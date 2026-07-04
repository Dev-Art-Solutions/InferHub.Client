using System.Net;
using System.Text.Json;
using InferHub.Client.Configuration;
using InferHub.Client.Exceptions;
using InferHub.Client.Extensions;
using InferHub.Client.Models.Ollama;
using Microsoft.Extensions.DependencyInjection;

namespace InferHub.Client.Tests;

public class InferHubClientTests
{
    private static (InferHubClient Client, FakeHttpMessageHandler Handler) CreateClient(HttpStatusCode status, string body)
    {
        var handler = new FakeHttpMessageHandler(status, body);
        var http = new HttpClient(handler) { BaseAddress = new Uri("http://localhost:5080/") };
        return (new InferHubClient(http), handler);
    }

    [Fact]
    public async Task ListModelsAsync_returns_models_on_200()
    {
        var (client, handler) = CreateClient(HttpStatusCode.OK, """{"models":[{"name":"llama3","digest":"abc","size":1234}]}""");

        var response = await client.ListModelsAsync();

        Assert.NotNull(response);
        Assert.Single(response.Models);
        Assert.Equal("llama3", response.Models[0].Name);
        Assert.Equal(HttpMethod.Get, handler.Requests[0].Method);
        Assert.EndsWith("api/tags", handler.Requests[0].RequestUri!.ToString());
    }

    [Fact]
    public async Task ChatAsync_forces_stream_false_and_deserializes_message()
    {
        const string body = """{"model":"llama3","message":{"role":"assistant","content":"hi"},"done":true,"eval_count":1}""";
        var (client, handler) = CreateClient(HttpStatusCode.OK, body);

        var response = await client.ChatAsync(new ChatRequest
        {
            Model = "llama3",
            Stream = true,
            Messages = new[] { new ChatMessage { Role = "user", Content = "hi" } }
        });

        Assert.Equal("assistant", response.Message?.Role);
        Assert.Equal("hi", response.Message?.Content);
        Assert.True(response.Done);

        var sent = JsonDocument.Parse(handler.RequestBodies[0]).RootElement;
        Assert.False(sent.GetProperty("stream").GetBoolean());
    }

    [Fact]
    public async Task GenerateAsync_forces_stream_false_and_returns_response_text()
    {
        var (client, _) = CreateClient(HttpStatusCode.OK, """{"model":"llama3","response":"pong","done":true}""");

        var result = await client.GenerateAsync(new GenerateRequest { Model = "llama3", Prompt = "ping" });

        Assert.Equal("pong", result.Response);
    }

    [Fact]
    public async Task GetStatusAsync_deserializes_version_and_nodes()
    {
        var (client, _) = CreateClient(HttpStatusCode.OK, """
            {
              "coordinatorVersion": "2.0.0",
              "nowUtc": "2026-07-03T00:00:00Z",
              "uptimeSeconds": 42.5,
              "nodes": [ { "nodeId": "n1", "name": "alpha", "inFlight": 2 } ],
              "models": [ { "name": "llama3" } ]
            }
            """);

        var status = await client.GetStatusAsync();

        Assert.Equal("2.0.0", status.CoordinatorVersion);
        Assert.Equal(42.5, status.UptimeSeconds);
        Assert.Single(status.Nodes!);
        Assert.Equal("alpha", status.Nodes![0].Name);
    }

    [Fact]
    public async Task PingAsync_returns_true_on_success()
    {
        var (client, _) = CreateClient(HttpStatusCode.OK, "OK");
        Assert.True(await client.PingAsync());
    }

    [Fact]
    public async Task PingAsync_returns_false_on_non_success()
    {
        var (client, _) = CreateClient(HttpStatusCode.ServiceUnavailable, "down");
        Assert.False(await client.PingAsync());
    }

    [Fact]
    public async Task Chat_404_surfaces_InferHubException_with_error_body()
    {
        var (client, _) = CreateClient(HttpStatusCode.NotFound, """{"error":"model 'nope' not found"}""");

        var ex = await Assert.ThrowsAsync<InferHubException>(() =>
            client.ChatAsync(new ChatRequest { Model = "nope", Messages = Array.Empty<ChatMessage>() }));

        Assert.Equal(HttpStatusCode.NotFound, ex.StatusCode);
        Assert.Equal("model 'nope' not found", ex.Message);
    }

    [Fact]
    public async Task Chat_401_surfaces_InferHubException()
    {
        var (client, _) = CreateClient(HttpStatusCode.Unauthorized, """{"error":"invalid api key"}""");

        var ex = await Assert.ThrowsAsync<InferHubException>(() =>
            client.ChatAsync(new ChatRequest { Model = "x" }));

        Assert.Equal(HttpStatusCode.Unauthorized, ex.StatusCode);
    }

    [Fact]
    public async Task Chat_400_uses_raw_body_when_no_error_field()
    {
        var (client, _) = CreateClient(HttpStatusCode.BadRequest, "model is required");

        var ex = await Assert.ThrowsAsync<InferHubException>(() =>
            client.ChatAsync(new ChatRequest()));

        Assert.Equal(HttpStatusCode.BadRequest, ex.StatusCode);
        Assert.Equal("model is required", ex.Message);
    }

    [Fact]
    public void AddInferHubClient_registers_typed_client()
    {
        var services = new ServiceCollection();
        services.AddInferHubClient(o =>
        {
            o.BaseAddress = new Uri("http://localhost:5080");
            o.ApiKey = "abc";
        });

        var provider = services.BuildServiceProvider();
        var client = provider.GetRequiredService<IInferHubClient>();

        Assert.IsType<InferHubClient>(client);
    }

    [Fact]
    public void AddInferHubClient_throws_without_configure()
    {
        var services = new ServiceCollection();
        Assert.Throws<ArgumentNullException>(() => services.AddInferHubClient(null!));
    }

    [Fact]
    public async Task Bearer_handler_attaches_api_key()
    {
        var handler = new FakeHttpMessageHandler(HttpStatusCode.OK, """{"models":[]}""");
        var bearer = new InferHub.Client.Http.BearerAuthorizationHandler(new InferHubClientOptions { ApiKey = "secret-key" })
        {
            InnerHandler = handler
        };
        var http = new HttpClient(bearer) { BaseAddress = new Uri("http://localhost:5080/") };
        var client = new InferHubClient(http);

        await client.ListModelsAsync();

        var auth = handler.Requests[0].Headers.Authorization;
        Assert.NotNull(auth);
        Assert.Equal("Bearer", auth!.Scheme);
        Assert.Equal("secret-key", auth.Parameter);
    }

    [Fact]
    public async Task ChatStreamAsync_yields_chunks_and_stops_on_done()
    {
        var handler = new StreamingHttpMessageHandler();
        var http = new HttpClient(handler) { BaseAddress = new Uri("http://localhost:5080/") };
        var client = new InferHubClient(http);

        handler.EnqueueLine("""{"model":"llama3","message":{"role":"assistant","content":"hel"},"done":false}""");
        handler.EnqueueLine("""{"model":"llama3","message":{"role":"assistant","content":"lo"},"done":false}""");
        handler.EnqueueLine("""{"model":"llama3","message":{"role":"assistant","content":"!"},"done":true}""");
        handler.EnqueueLine("""{"model":"llama3","message":{"role":"assistant","content":"IGNORED"},"done":false}""");
        handler.Complete();

        var deltas = new List<string>();
        await foreach (var chunk in client.ChatStreamAsync(new ChatRequest { Model = "llama3" }))
        {
            deltas.Add(chunk.Message?.Content ?? string.Empty);
        }

        Assert.Equal(new[] { "hel", "lo", "!" }, deltas);

        var sent = JsonDocument.Parse(handler.RequestBodies[0]).RootElement;
        Assert.True(sent.GetProperty("stream").GetBoolean());
    }

    [Fact]
    public async Task ChatStreamAsync_surfaces_terminal_error_chunk_as_exception()
    {
        var handler = new StreamingHttpMessageHandler();
        var http = new HttpClient(handler) { BaseAddress = new Uri("http://localhost:5080/") };
        var client = new InferHubClient(http);

        handler.EnqueueLine("""{"model":"llama3","message":{"role":"assistant","content":"partial"},"done":false}""");
        handler.EnqueueLine("""{"error":"node dropped mid-stream","done":true}""");
        handler.Complete();

        var seen = new List<string?>();
        var ex = await Assert.ThrowsAsync<InferHubException>(async () =>
        {
            await foreach (var chunk in client.ChatStreamAsync(new ChatRequest { Model = "llama3" }))
            {
                seen.Add(chunk.Message?.Content);
            }
        });

        Assert.Equal("node dropped mid-stream", ex.Message);
        Assert.Single(seen);
        Assert.Equal("partial", seen[0]);
    }

    [Fact]
    public async Task GenerateStreamAsync_yields_response_deltas_and_stops_on_done()
    {
        var handler = new StreamingHttpMessageHandler();
        var http = new HttpClient(handler) { BaseAddress = new Uri("http://localhost:5080/") };
        var client = new InferHubClient(http);

        handler.EnqueueLine("""{"model":"llama3","response":"po","done":false}""");
        handler.EnqueueLine("""{"model":"llama3","response":"ng","done":true}""");
        handler.Complete();

        var deltas = new List<string>();
        await foreach (var chunk in client.GenerateStreamAsync(new GenerateRequest { Model = "llama3", Prompt = "ping" }))
        {
            deltas.Add(chunk.Response ?? string.Empty);
        }

        Assert.Equal(new[] { "po", "ng" }, deltas);
    }

    [Fact]
    public async Task ChatStreamAsync_cancellation_throws_promptly_between_chunks()
    {
        var handler = new StreamingHttpMessageHandler();
        var http = new HttpClient(handler) { BaseAddress = new Uri("http://localhost:5080/") };
        var client = new InferHubClient(http);

        using var cts = new CancellationTokenSource();
        handler.EnqueueLine("""{"model":"llama3","message":{"role":"assistant","content":"first"},"done":false}""");

        await Assert.ThrowsAnyAsync<OperationCanceledException>(async () =>
        {
            await foreach (var chunk in client.ChatStreamAsync(new ChatRequest { Model = "llama3" }, cts.Token))
            {
                cts.Cancel();
            }
        });
    }

    [Fact]
    public async Task Bearer_handler_leaves_authorization_off_when_no_key()
    {
        var handler = new FakeHttpMessageHandler(HttpStatusCode.OK, """{"models":[]}""");
        var bearer = new InferHub.Client.Http.BearerAuthorizationHandler(new InferHubClientOptions())
        {
            InnerHandler = handler
        };
        var http = new HttpClient(bearer) { BaseAddress = new Uri("http://localhost:5080/") };
        var client = new InferHubClient(http);

        await client.ListModelsAsync();

        Assert.Null(handler.Requests[0].Headers.Authorization);
    }
}
