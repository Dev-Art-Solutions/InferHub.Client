# InferHub.Client v0.6.0 — social drafts

Version + facts verified at release time (2026-07-08):
- Tag `v0.6.0` pushed, release workflow green (pack + NuGet push + GitHub release).
- GitHub release: https://github.com/Dev-Art-Solutions/InferHub.Client/releases/tag/v0.6.0
- Blog post: inferhub-client-admin — created EN visible, BG hidden (ID 6a4e2c4f35bea663759f149c).
- Product site C# client section refreshed (commit 4adedde).

## Facebook (facebook.com/DevArtSolutions)

> InferHub.Client v0.6.0 is out. Phase 5 adds the admin client: drive the InferHub fleet
> and its vector store from C#, on a separate IInferHubAdminClient authenticated with an
> admin key — a client key alone never even sees the admin methods.
>
> Fleet ops are the headline: list nodes with their in-flight counts and cordon state,
> then cordon → drain → uncordon a GPU box in three lines (DrainAsync cordons and polls
> until in-flight work hits zero — the node finishes what it has, it just stops getting
> new jobs). Vector collections get full lifecycle too: create, drop, rebuild, and a
> one-read health check with replica placement and an underReplicated flag. And
> StreamAdminEventsAsync tails the coordinator's live SSE feed — fleet snapshots plus
> vector.* replication events — with optional reconnect + backoff.
>
> Install: dotnet add package InferHub.Client --version 0.6.0
> Repo: github.com/Dev-Art-Solutions/InferHub.Client

## X (twitter/X)

> InferHub.Client v0.6.0 — drive the fleet from C#. Cordon → drain → uncordon a GPU node
> in three lines, manage vector collections with replica health, and tail the live admin
> SSE stream with reconnect. Separate admin interface + key; a client key never sees it.
> nuget.org/packages/InferHub.Client

## Notes for Iliya

- Core hook: "the operator side of the mesh, typed, from C# — cordon/drain/uncordon in
  three lines."
- Security angle worth keeping: separate interface + separate admin key mirrors the
  coordinator's auth split; it's a design point, not an accident.
- Honest caveats: DrainAsync is client-side (cordon + poll — there is no server drain
  endpoint); vector.* SSE events are live-only, not replayed after a reconnect.
- If you want a screenshot: `samples/FleetOps` against a local coordinator shows nodes,
  a drain, the collection list and a 15s live event tail in one terminal frame.
