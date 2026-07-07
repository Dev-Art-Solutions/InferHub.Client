Phase 3 of the InferHub client roadmap. On top of the v0.3.0 inference + embeddings
surface, this release adds the vector data-plane, so you can store and search vectors on
the mesh from typed C# — text in, ranked matches out, without holding an embedding model
client-side.

## What ships

- `UpsertAsync` writes a record to `POST /api/vector/{collection}/upsert`. Supply a raw
  vector with `VectorUpsert.FromVector(id, vector)` or text to embed on a node with
  `VectorUpsert.FromText(id, text, model)`; `.WithPayload(obj)` and `.WithMetadata(map)`
  attach an opaque payload and filterable metadata.
- `QueryAsync` and `RetrieveAsync` search `POST /api/vector/{collection}/query` and
  `.../retrieve`. Build the body with `VectorQuery.FromVector` / `VectorQuery.FromText`
  (`.WithFilter(map)` for metadata filtering); both return the ranked
  `IReadOnlyList<VectorMatch>`, closest first.
- `GetRecordAsync` reads a record by id and returns `null` on a 404; `DeleteRecordAsync`
  removes one and returns `false` on a 404. Every other non-success status is an
  `InferHubException`. Collection and id path segments are URL-escaped.
- `VectorRecord` / `VectorMatch` expose `payload` as a `JsonElement?`; call
  `payload.As<T>()` to deserialize it into your own type.
- `samples/MiniRag` — embed a handful of docs, query by text, read the top hit's payload,
  in one console run.

## Install

```
dotnet add package InferHub.Client --version 0.4.0
```

## Minimal example

```csharp
using InferHub.Client.Models.Vector;

await client.UpsertAsync("docs", VectorUpsert
    .FromText("doc-1", "InferHub is a self-hosted inference mesh.", "nomic-embed-text"));

var matches = await client.QueryAsync("docs",
    VectorQuery.FromText("what is InferHub?", "nomic-embed-text", k: 3));
```

Needs the coordinator running with `VectorStore:Enabled=true` and the collection already
created. RAG retrieval headers and the admin client land in later phases.

**Full Changelog**: https://github.com/Dev-Art-Solutions/InferHub.Client/compare/v0.3.0...v0.4.0
