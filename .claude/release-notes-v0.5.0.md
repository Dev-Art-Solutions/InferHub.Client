Phase 4 of the InferHub client roadmap. On top of the v0.4.0 vector data-plane, this
release adds opt-in RAG retrieval to chat and generate: the coordinator pulls the top
matches from a collection, grounds your prompt, and tells you which records it used — all
driven from typed C# with one options object. Orchestration happens on the hub; compute
happens on the fleet.

## What ships

- A new `InferHubCallOptions` carries per-call retrieval and routing. `RetrievalOptions
  { Collection, K?, Model? }` maps to the coordinator's `X-InferHub-Retrieve[-K|-Model]`
  headers; `ConversationId` maps to `X-InferHub-Conversation` for sticky routing. Use the
  `InferHubCallOptions.ForRetrieval(collection, k, model)` / `.ForConversation(id)`
  shorthands, or set the properties directly.
- New overloads of `ChatAsync`, `GenerateAsync`, `ChatStreamAsync` and `GenerateStreamAsync`
  accept the options. The existing signatures are unchanged — a call without options behaves
  exactly as before.
- `ChatResponse.SourceIds` / `GenerateResponse.SourceIds` expose the grounding record ids,
  parsed from the `X-InferHub-Sources` response header. Blocking calls set it on the result;
  streaming calls stamp the same list on every chunk.
- Retrieval failures (`424 Failed Dependency` — retrieval unavailable, or `OnMissing=error`
  with no hits) surface as `InferHubRetrievalException`, a subclass of `InferHubException`,
  so you can catch them distinctly while a broad handler still works.
- `samples/GroundedChat` — seed a collection, ask a question with retrieval on, print the
  answer alongside the source ids, in one console run.

## Install

```
dotnet add package InferHub.Client --version 0.5.0
```

## Minimal example

```csharp
var response = await client.ChatAsync(
    new ChatRequest
    {
        Model = "llama3",
        Messages = new[] { new ChatMessage { Role = "user", Content = "How do nodes connect?" } },
    },
    InferHubCallOptions.ForRetrieval("docs", k: 3, model: "nomic-embed-text"));

Console.WriteLine(response.Message?.Content);
Console.WriteLine($"grounded on: {string.Join(", ", response.SourceIds ?? [])}");
```

Needs the coordinator running with `VectorStore:Enabled=true` and the collection already
populated. The admin client (fleet + vector ops) lands next.

**Full Changelog**: https://github.com/Dev-Art-Solutions/InferHub.Client/compare/v0.4.0...v0.5.0
