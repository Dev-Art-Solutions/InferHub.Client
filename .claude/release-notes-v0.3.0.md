Phase 2 of the InferHub client roadmap. On top of the v0.2.0 blocking + streaming
inference surface, this release adds the two embeddings endpoints so you can drive
`nomic-embed-text` (or any other embedding model on the mesh) directly from typed
C# calls.

## What ships

- `EmbedAsync` targets the modern batch endpoint (`POST /api/embed`). Input is a JSON
  string or a `string[]`; the response returns one vector per input, in order.
  `EmbedRequest.FromText(model, text)` and `EmbedRequest.FromTexts(model, texts)` hide
  the `JsonElement` construction so the common cases are one line.
- `EmbedLegacyAsync` wraps the legacy `POST /api/embeddings` (single `prompt`) and
  returns a single `float[]` — kept for drop-in Ollama callers.
- `404` (no embedding node), `400` (bad body) and `502` (node dropped mid-flight) all
  surface as `InferHubException` with the raw status preserved. An empty vector list
  on a 200 response is treated as malformed and thrown, so you never get a silent
  zero-vector result.
- `samples/Embeddings` — single, batch and legacy shapes in one console run.

## Install

```
dotnet add package InferHub.Client --version 0.3.0
```

## Minimal example

```csharp
var batch = await client.EmbedAsync(EmbedRequest.FromTexts(
    "nomic-embed-text",
    new[] { "InferHub", "self-hosted", "inference mesh" }));

// batch.Embeddings.Length == 3, one vector per input, same order.
```

Vector data plane, RAG headers and admin land in later phases.

**Full Changelog**: https://github.com/Dev-Art-Solutions/InferHub.Client/compare/v0.2.0...v0.3.0
