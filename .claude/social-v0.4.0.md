# InferHub.Client v0.4.0 — social drafts

Version + facts to verify at release time (2026-07-06):
- Tag `v0.4.0` pushed, release workflow green.
- NuGet: confirm `Your package was pushed.` in the workflow log; flat-container index
  catches up within a few minutes.
- GitHub release: https://github.com/Dev-Art-Solutions/InferHub.Client/releases/tag/v0.4.0
- Blog post: inferhub-client-vectors (slug) — set EN visible, BG hidden, once written.

## Facebook (facebook.com/DevArtSolutions)

> InferHub.Client v0.4.0 is out. Phase 3 adds the vector data-plane on top of the v0.3.0
> chat, generate and embeddings surface, so you can store and search vectors on the mesh
> directly from typed C#. Upsert text and the coordinator embeds it on a node for you;
> query by text and get ranked matches back — no embedding model to hold client-side.
>
> `VectorUpsert.FromText(id, text, model)` to write, `VectorQuery.FromText(text, model, k)`
> to search, plus get/delete by id and a `payload.As<T>()` helper to read your own type
> back out. It needs a coordinator with the vector store enabled (VectorStore:Enabled=true)
> and the collection created. RAG retrieval and admin land in later phases.
>
> Install: dotnet add package InferHub.Client --version 0.4.0
> Repo: github.com/Dev-Art-Solutions/InferHub.Client

## X (twitter/X)

> InferHub.Client v0.4.0 — the vector data-plane from C#. Text in, ranked matches out:
> UpsertAsync / QueryAsync / RetrieveAsync + get/delete by id, payload.As<T>(). The hub
> embeds text on a node, so you hold no model client-side. Needs VectorStore:Enabled=true.
>
> nuget.org/packages/InferHub.Client

## Notes for Iliya

- Core hook: "text in, matches out" — the coordinator does the embedding, the client stays tiny.
- Honest caveat worth keeping: it needs `VectorStore:Enabled=true` and a pre-created collection.
- If you want a screenshot: `samples/MiniRag` run against a local coordinator shows the
  embed → query → payload loop in one terminal frame.
