using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text.Json;
using InferHub.Client.Exceptions;
using InferHub.Client.Models;
using InferHub.Client.Models.Ollama;

namespace InferHub.Client;

/// <inheritdoc cref="IInferHubClient"/>
public sealed class InferHubClient : IInferHubClient
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
    };

    private readonly HttpClient httpClient;

    /// <summary>Create a new client. Prefer <c>services.AddInferHubClient(...)</c> in DI.</summary>
    public InferHubClient(HttpClient httpClient)
    {
        this.httpClient = httpClient;
    }

    /// <inheritdoc/>
    public async Task<TagsResponse> ListModelsAsync(CancellationToken cancellationToken = default)
    {
        return await GetAsync<TagsResponse>("api/tags", cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<ChatResponse> ChatAsync(ChatRequest request, CancellationToken cancellationToken = default)
    {
        request.Stream = false;
        return await PostAsync<ChatRequest, ChatResponse>("api/chat", request, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<GenerateResponse> GenerateAsync(GenerateRequest request, CancellationToken cancellationToken = default)
    {
        request.Stream = false;
        return await PostAsync<GenerateRequest, GenerateResponse>("api/generate", request, cancellationToken);
    }

    /// <inheritdoc/>
    public IAsyncEnumerable<ChatResponse> ChatStreamAsync(ChatRequest request, CancellationToken cancellationToken = default)
    {
        request.Stream = true;
        return StreamNdjsonAsync<ChatRequest, ChatResponse>(
            "api/chat",
            request,
            static chunk => (chunk.Done == true, chunk.Error),
            cancellationToken);
    }

    /// <inheritdoc/>
    public IAsyncEnumerable<GenerateResponse> GenerateStreamAsync(GenerateRequest request, CancellationToken cancellationToken = default)
    {
        request.Stream = true;
        return StreamNdjsonAsync<GenerateRequest, GenerateResponse>(
            "api/generate",
            request,
            static chunk => (chunk.Done == true, chunk.Error),
            cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<EmbedResponse> EmbedAsync(EmbedRequest request, CancellationToken cancellationToken = default)
    {
        var response = await PostAsync<EmbedRequest, EmbedResponse>("api/embed", request, cancellationToken);
        if (response.Embeddings is null || response.Embeddings.Length == 0)
        {
            throw new InferHubException(System.Net.HttpStatusCode.OK, "embed response had no vectors", string.Empty);
        }

        return response;
    }

    /// <inheritdoc/>
    public async Task<EmbeddingsResponse> EmbedLegacyAsync(EmbeddingsRequest request, CancellationToken cancellationToken = default)
    {
        var response = await PostAsync<EmbeddingsRequest, EmbeddingsResponse>("api/embeddings", request, cancellationToken);
        if (response.Embedding is null || response.Embedding.Length == 0)
        {
            throw new InferHubException(System.Net.HttpStatusCode.OK, "embeddings response had no vector", string.Empty);
        }

        return response;
    }

    /// <inheritdoc/>
    public async Task<StatusResponse> GetStatusAsync(CancellationToken cancellationToken = default)
    {
        return await GetAsync<StatusResponse>("api/status", cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<bool> PingAsync(CancellationToken cancellationToken = default)
    {
        using var response = await httpClient.GetAsync("health", cancellationToken);
        return response.IsSuccessStatusCode;
    }

    private async Task<TResult> GetAsync<TResult>(string path, CancellationToken cancellationToken)
    {
        using var response = await httpClient.GetAsync(path, cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);
        var result = await response.Content.ReadFromJsonAsync<TResult>(JsonOptions, cancellationToken);
        return result ?? throw new InferHubException(response.StatusCode, "empty response body", string.Empty);
    }

    private async Task<TResult> PostAsync<TRequest, TResult>(string path, TRequest body, CancellationToken cancellationToken)
    {
        using var response = await httpClient.PostAsJsonAsync(path, body, JsonOptions, cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);
        var result = await response.Content.ReadFromJsonAsync<TResult>(JsonOptions, cancellationToken);
        return result ?? throw new InferHubException(response.StatusCode, "empty response body", string.Empty);
    }

    private async IAsyncEnumerable<TChunk> StreamNdjsonAsync<TRequest, TChunk>(
        string path,
        TRequest body,
        Func<TChunk, (bool Done, string? Error)> inspect,
        [EnumeratorCancellation] CancellationToken cancellationToken)
        where TChunk : class
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, path)
        {
            Content = JsonContent.Create(body, mediaType: null, options: JsonOptions)
        };
        using var response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);

        using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var reader = new StreamReader(stream);

        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var line = await reader.ReadLineAsync(cancellationToken);
            if (line is null)
            {
                yield break;
            }

            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            TChunk? chunk;
            try
            {
                chunk = JsonSerializer.Deserialize<TChunk>(line, JsonOptions);
            }
            catch (JsonException ex)
            {
                throw new InferHubException(response.StatusCode, $"Malformed NDJSON chunk: {ex.Message}", line);
            }

            if (chunk is null)
            {
                continue;
            }

            var (done, error) = inspect(chunk);
            if (!string.IsNullOrEmpty(error))
            {
                throw new InferHubException(response.StatusCode, error, line);
            }

            yield return chunk;

            if (done)
            {
                yield break;
            }
        }
    }

    private static async Task EnsureSuccessAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        if (response.IsSuccessStatusCode)
        {
            return;
        }

        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        var message = TryExtractErrorMessage(body) ?? $"InferHub request failed with status {(int)response.StatusCode} ({response.StatusCode}).";
        throw new InferHubException(response.StatusCode, message, body);
    }

    private static string? TryExtractErrorMessage(string body)
    {
        if (string.IsNullOrWhiteSpace(body))
        {
            return null;
        }

        try
        {
            using var doc = JsonDocument.Parse(body);
            if (doc.RootElement.ValueKind == JsonValueKind.Object
                && doc.RootElement.TryGetProperty("error", out var errorElement)
                && errorElement.ValueKind == JsonValueKind.String)
            {
                return errorElement.GetString();
            }
        }
        catch (JsonException)
        {
            // Non-JSON body — fall through to the raw string.
        }

        return body;
    }
}
