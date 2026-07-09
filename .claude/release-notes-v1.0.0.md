InferHub.Client 1.0.0 — the hardening-and-stability release. No new endpoints; instead we
froze the public API, made serialization trim- and AOT-friendly, and added optional
transient retries. The full mesh surface — blocking + streaming inference, embeddings,
the vector data plane, opt-in RAG retrieval, and the admin client — now ships from one
small package with a semver contract behind it.

## What ships

- **Stable 1.0 API.** Every public signature is frozen for the `1.x` line. New capability
  lands as additive overloads (the way per-call RAG options did), so existing call sites
  keep compiling across the whole major version. Client versions stay independent of the
  coordinator's; `1.0.0` targets the coordinator's `v2.x` HTTP surface.
- **Trim- and AOT-friendly serialization.** The typed request/response surface now
  serializes through a source-generated `JsonSerializerContext` instead of runtime
  reflection, and the library is marked `<IsAotCompatible>true</IsAotCompatible>` — it
  builds clean under the trim and Native-AOT analysers. The two generic payload escape
  hatches (`VectorUpsert.WithPayload<T>`, `JsonElement?.As<T>()`) gained
  `JsonTypeInfo<T>` overloads for an AOT-safe path; the reflection overloads are annotated
  so the analyser points callers at them.
- **Optional transient retries, off by default.** Set `Options.MaxRetryAttempts` (with
  `RetryBaseDelay` / `MaxRetryDelay`) to ride out brief coordinator restarts. Retries apply
  **only to idempotent requests** — `GET`/`HEAD` that fail with a connection error or a
  `5xx`/`408` — so a chat, generate, embed, upsert or delete is never silently re-run, and
  a stream is never retried mid-flight.
- **Opt-in integration tests.** A round-trip suite (health, status, model list, and a real
  chat) runs against a live coordinator when `INFERHUB_TEST_BASEADDRESS` is set, and
  self-skips otherwise so the default test run stays hermetic.
- **Docs.** README refreshed with NuGet/build badges, a full method → endpoint table, and
  new Resilience, Trimming & AOT, and Versioning sections. XML docs cover the public API
  (`CS1591` is an error in Release).

## Install

```
dotnet add package InferHub.Client --version 1.0.0
```

## Minimal example

```csharp
services.AddInferHubClient(o =>
{
    o.BaseAddress = new Uri("http://localhost:5080");
    o.ApiKey = "<client-key>";
    o.MaxRetryAttempts = 3; // optional; 0 = off (default)
});

var client = provider.GetRequiredService<IInferHubClient>();
var chat = await client.ChatAsync(new ChatRequest
{
    Model = "llama3",
    Messages = new[] { new ChatMessage { Role = "user", Content = "Say hi in one word." } }
});
Console.WriteLine(chat.Message?.Content);
```

**Full Changelog**: https://github.com/Dev-Art-Solutions/InferHub.Client/compare/v0.6.0...v1.0.0
