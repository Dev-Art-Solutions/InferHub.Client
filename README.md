# InferHub.Client

A small, typed .NET client for [InferHub](https://github.com/Dev-Art-Solutions/InferHub) —
a self-hosted, Ollama-compatible inference mesh.

Point it at a coordinator, pass a Bearer token, and call chat, generate, model listing
and status from C# with typed requests, dependency injection, and no heavy dependencies.

> **v0.3.0** — blocking + streaming inference, and embeddings (batch + legacy). Vector
> data plane, RAG retrieval and admin follow in later phases.

## Install

```
dotnet add package InferHub.Client
```

Targets `net9.0` and `net10.0`.

## Quick start

```csharp
using InferHub.Client;
using InferHub.Client.Configuration;
using InferHub.Client.Models.Ollama;
using Microsoft.Extensions.DependencyInjection;

var services = new ServiceCollection();
services.AddInferHubClient(o =>
{
    o.BaseAddress = new Uri("http://localhost:5080");
    o.ApiKey = "<your-client-api-key>";
});

var provider = services.BuildServiceProvider();
var client = provider.GetRequiredService<IInferHubClient>();

var chat = await client.ChatAsync(new ChatRequest
{
    Model = "llama3",
    Messages = new[]
    {
        new ChatMessage { Role = "user", Content = "Say hi in one word." }
    },
    Stream = false
}, CancellationToken.None);

Console.WriteLine(chat.Message?.Content);
```

## What ships in v0.3.0

| Method | Endpoint |
|---|---|
| `ListModelsAsync` | `GET /api/tags` |
| `GenerateAsync` (blocking) | `POST /api/generate` with `stream:false` |
| `ChatAsync` (blocking) | `POST /api/chat` with `stream:false` |
| `ChatStreamAsync` | `POST /api/chat` with `stream:true` (NDJSON → `IAsyncEnumerable<ChatResponse>`) |
| `GenerateStreamAsync` | `POST /api/generate` with `stream:true` (NDJSON → `IAsyncEnumerable<GenerateResponse>`) |
| `EmbedAsync` | `POST /api/embed` (batch — single string or string[]) |
| `EmbedLegacyAsync` | `POST /api/embeddings` (legacy single `prompt`) |
| `GetStatusAsync` | `GET /api/status` |
| `PingAsync` | `GET /health` |

### Streaming

```csharp
await foreach (var chunk in client.ChatStreamAsync(new ChatRequest
{
    Model = "llama3",
    Messages = new[] { new ChatMessage { Role = "user", Content = "Stream me a haiku." } }
}, cancellationToken))
{
    Console.Write(chunk.Message?.Content);
}
```

The enumerator stops as soon as a chunk arrives with `done:true`. A terminal error
chunk (`{ "error": …, "done": true }`) is surfaced as `InferHubException` — the client
never retries mid-stream, so a partial answer plus a clean exception is the contract.
Cancelling the token throws promptly out of the `await foreach`.

Request models carry an extension bag (`AdditionalProperties`), so any unknown fields
from the Ollama contract pass through untouched — you can hand-set `options`, `format`,
tool definitions, etc. without waiting on the client to type them.

### Embeddings

```csharp
// Single input.
var single = await client.EmbedAsync(
    EmbedRequest.FromText("nomic-embed-text", "hello, world"));

// Batch — one vector per input, same order.
var batch = await client.EmbedAsync(EmbedRequest.FromTexts(
    "nomic-embed-text",
    new[] { "InferHub", "self-hosted", "inference mesh" }));

Console.WriteLine(batch.Embeddings.Length);   // 3
Console.WriteLine(batch.Embeddings[0].Length); // model dimension
```

`EmbedAsync` targets the modern batch endpoint (`/api/embed`); `EmbedLegacyAsync` wraps
`/api/embeddings` for drop-in Ollama callers. An empty vector list on a 200 response is
treated as malformed and surfaced as `InferHubException` — the client never returns a
silent zero-vector result.

## Auth

- Non-loopback calls need `ApiKey` (attached as `Authorization: Bearer <key>` by a
  `DelegatingHandler`).
- Loopback calls to the coordinator skip auth by default (unless the coordinator sets
  `Auth:RequireAuthForLoopback=true`).
- `/health` is always open.
- Admin routes require a **separate** `AdminApiKey` and land on a dedicated interface in
  a later phase; a client key alone never surfaces admin methods.

## Errors

Any non-success HTTP response is surfaced as `InferHubException`, carrying:

- `StatusCode` — the HTTP status
- `Message` — the coordinator's `{ "error": "…" }` body if present

The client treats `404` (model missing) and `424 Failed Dependency` (retrieval unavailable,
added in a later phase) as distinct signals worth checking with `StatusCode`.

## Links

- InferHub server: <https://github.com/Dev-Art-Solutions/InferHub>
- Product page: <https://inferhub.devart.solutions>
- Blog: <https://blog.devart.solutions>

## License

MIT — see [LICENSE](LICENSE).
