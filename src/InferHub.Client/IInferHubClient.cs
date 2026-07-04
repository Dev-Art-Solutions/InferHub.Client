using InferHub.Client.Models;
using InferHub.Client.Models.Ollama;

namespace InferHub.Client;

/// <summary>
/// Client for talking to an InferHub coordinator over its Ollama-compatible HTTP API.
/// v0.1.0 covers blocking chat/generate, model listing, status and health.
/// </summary>
public interface IInferHubClient
{
    /// <summary>
    /// List models advertised by the mesh. Wraps <c>GET /api/tags</c>.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task<TagsResponse> ListModelsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Blocking chat call — <c>POST /api/chat</c> with <c>stream:false</c>.
    /// Streaming lands in a later phase.
    /// </summary>
    /// <param name="request">Chat request. <see cref="ChatRequest.Stream"/> is forced to <c>false</c>.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task<ChatResponse> ChatAsync(ChatRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Blocking generate call — <c>POST /api/generate</c> with <c>stream:false</c>.
    /// Streaming lands in a later phase.
    /// </summary>
    /// <param name="request">Generate request. <see cref="GenerateRequest.Stream"/> is forced to <c>false</c>.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task<GenerateResponse> GenerateAsync(GenerateRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Streaming chat call — <c>POST /api/chat</c> with <c>stream:true</c>. Yields one
    /// <see cref="ChatResponse"/> per NDJSON chunk; the enumerator stops after the chunk
    /// with <c>done:true</c>. A terminal error chunk (<c>{ "error": …, "done": true }</c>)
    /// is surfaced as an <see cref="Exceptions.InferHubException"/> instead of a silent stop.
    /// </summary>
    /// <param name="request">Chat request. <see cref="ChatRequest.Stream"/> is forced to <c>true</c>.</param>
    /// <param name="cancellationToken">Cancels the read loop; a cancelled token throws promptly.</param>
    IAsyncEnumerable<ChatResponse> ChatStreamAsync(ChatRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Streaming generate call — <c>POST /api/generate</c> with <c>stream:true</c>. Yields
    /// one <see cref="GenerateResponse"/> per NDJSON chunk; stops after <c>done:true</c>.
    /// A terminal error chunk is surfaced as an <see cref="Exceptions.InferHubException"/>.
    /// </summary>
    /// <param name="request">Generate request. <see cref="GenerateRequest.Stream"/> is forced to <c>true</c>.</param>
    /// <param name="cancellationToken">Cancels the read loop; a cancelled token throws promptly.</param>
    IAsyncEnumerable<GenerateResponse> GenerateStreamAsync(GenerateRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Coordinator/fleet snapshot — <c>GET /api/status</c>.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task<StatusResponse> GetStatusAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Cheap liveness probe — <c>GET /health</c>. Returns <c>true</c> on 2xx, <c>false</c> otherwise.
    /// Never throws for non-success statuses — throws only on transport errors.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task<bool> PingAsync(CancellationToken cancellationToken = default);
}
