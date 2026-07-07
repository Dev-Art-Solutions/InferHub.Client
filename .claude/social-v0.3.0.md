# InferHub.Client v0.3.0 — social drafts

Version + facts verified at release time (2026-07-05):
- Tag `v0.3.0` pushed, release workflow green.
- NuGet: package accepted (`Your package was pushed.`), flat-container index catching up.
- GitHub release: https://github.com/Dev-Art-Solutions/InferHub.Client/releases/tag/v0.3.0
- Blog post: https://blog.devart.solutions/blog/inferhub-client-embeddings (EN visible, BG hidden).

## Facebook (facebook.com/DevArtSolutions)

> InferHub.Client v0.3.0 is out. Phase 2 adds embeddings on top of the v0.2.0 chat and
> generate surface, so you can drive an embedding model on the mesh directly from typed
> C# — batch and single, both shapes covered. `EmbedRequest.FromTexts(model, texts)`
> sends a string[] and hands you one vector per input, in order.
>
> Missing embedding node? 404 as a typed exception. Empty vector list on a 200? Still
> thrown — the client never returns a silent zero-vector. Vector data plane, RAG
> retrieval headers and admin land in later phases.
>
> Install: dotnet add package InferHub.Client --version 0.3.0
> Repo: github.com/Dev-Art-Solutions/InferHub.Client
> Post: blog.devart.solutions/blog/inferhub-client-embeddings

## X (twitter/X)

> InferHub.Client v0.3.0 — embeddings from C#. EmbedAsync (batch, `input` is string or
> string[]) + EmbedLegacyAsync for drop-in callers. Typed errors, no silent zero
> vectors, one small package.
>
> nuget.org/packages/InferHub.Client
> blog.devart.solutions/blog/inferhub-client-embeddings

## Notes for Iliya

- Pattern-of-3 messaging: batch + legacy + one-line error model.
- If you want a screenshot: `samples/Embeddings` run against a local coordinator
  shows single + batch + legacy in one terminal frame.
- Cross-post to LinkedIn? The FB copy fits without changes.
