# InferHub.Client v1.0.0 — social drafts

Verify version + facts at release time before posting:
- Tag `v1.0.0` pushed, release workflow green (pack + NuGet push + GitHub release).
- GitHub release: https://github.com/Dev-Art-Solutions/InferHub.Client/releases/tag/v1.0.0
- Blog post: inferhub-client-1-0 — create EN visible, BG hidden (connector is insert-only,
  so create it visible in one shot).
- Product site C# client section refreshed for 1.0.

## Facebook (facebook.com/DevArtSolutions)

> InferHub.Client is 1.0. No new endpoints this time — this is the stability release. The
> whole mesh surface has been in place for a while (blocking + streaming inference,
> embeddings, the vector data plane, opt-in RAG, and the admin client), so 1.0 is about
> making it something you can build on.
>
> Three things landed. The public API is frozen for the 1.x line under semantic versioning —
> new features arrive as additive overloads, so your call sites keep compiling. Serialization
> moved from runtime reflection to a source-generated JSON context, so the library is now
> trim- and Native-AOT-friendly and builds clean under the analysers. And there's optional,
> off-by-default transient retry: turn it on and idempotent reads ride out a coordinator
> restart, while chats, upserts and streams are never silently re-run.
>
> Install: dotnet add package InferHub.Client --version 1.0.0
> Repo: github.com/Dev-Art-Solutions/InferHub.Client

## X (twitter/X)

> InferHub.Client 1.0 is out — the stability release. Public API frozen under semver,
> serialization moved to a source-generated JSON context (trim + Native-AOT friendly), and
> optional off-by-default retries for idempotent reads (never a chat, upsert, or a stream
> mid-flight). Full mesh surface, one small package.
> nuget.org/packages/InferHub.Client

## Notes for Iliya

- Core hook: "1.0 is the stability release — frozen API, AOT-friendly, safe retries — not
  a feature drop."
- Honest framing: no new endpoints. The value is the semver contract + AOT + resilience.
- Retry caveat worth keeping: idempotent GET/HEAD only, on connect errors and 5xx/408 —
  by design it never re-runs a mutation or retries a stream mid-flight.
- AOT caveat: the generic payload helpers need a JsonTypeInfo<T> overload to stay
  warning-free under trimming/AOT; the reflection overloads still work under JIT.
