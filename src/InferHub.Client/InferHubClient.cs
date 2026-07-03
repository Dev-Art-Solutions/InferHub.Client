using System.Net.Http.Json;
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
