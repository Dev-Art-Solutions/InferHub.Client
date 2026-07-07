# InferHub.Client v0.5.0 — social drafts

Version + facts to verify at release time (2026-07-07):
- Tag `v0.5.0` pushed, release workflow green.
- NuGet: confirm `Your package was pushed.` in the workflow log; flat-container index
  catches up within a few minutes.
- GitHub release: https://github.com/Dev-Art-Solutions/InferHub.Client/releases/tag/v0.5.0
- Blog post: inferhub-client-rag (slug) — set EN visible, BG hidden, once written.

## Facebook (facebook.com/DevArtSolutions)

> InferHub.Client v0.5.0 is out. Phase 4 adds opt-in RAG retrieval to chat and generate,
> on top of the v0.4.0 vector data-plane. Pass one options object and the coordinator pulls
> the top matches from a collection, grounds your prompt, and hands back the ids of the
> records it used — orchestration on the hub, compute on the fleet.
>
> `InferHubCallOptions.ForRetrieval(collection, k, model)` turns retrieval on for a call;
> the answer comes back with `SourceIds` alongside it (works for blocking and streaming). A
> call with no options behaves exactly like a plain chat, and a retrieval that can't be
> satisfied surfaces as a distinct `InferHubRetrievalException` (HTTP 424) rather than a
> silent, ungrounded answer. Needs a coordinator with the vector store enabled and the
> collection populated. The admin client lands next.
>
> Install: dotnet add package InferHub.Client --version 0.5.0
> Repo: github.com/Dev-Art-Solutions/InferHub.Client

## X (twitter/X)

> InferHub.Client v0.5.0 — opt-in RAG from C#. One options object grounds a chat/generate
> call in a collection; the answer comes back with SourceIds (blocking + streaming). No
> grounding? A distinct InferHubRetrievalException (424), never a silent ungrounded answer.
> Orchestration on the hub, compute on the fleet.
>
> nuget.org/packages/InferHub.Client

## Notes for Iliya

- Core hook: "opt-in RAG with one option object; sources come back with the answer."
- Framing worth keeping: "orchestration on the hub, compute on the fleet."
- Honest caveat: needs `VectorStore:Enabled=true` and a populated collection; retrieval
  failure is a clean 424, not a silent fallback.
- If you want a screenshot: `samples/GroundedChat` against a local coordinator shows the
  answer + source ids in one terminal frame.
