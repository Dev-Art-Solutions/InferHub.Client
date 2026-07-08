Phase 5 of the InferHub client roadmap. This release adds the admin client: drive the
fleet and the vector store's lifecycle from C# with the same small, typed surface —
list and cordon nodes, drain them before maintenance, create and drop collections,
watch replica health, and tail the coordinator's live event stream.

## What ships

- A new `IInferHubAdminClient`, registered by the same `AddInferHubClient(...)` call but
  authenticated with `AdminApiKey` — it is a separate interface, so a client key alone
  never surfaces admin methods. All routes live under `/api/admin/*` and are audited by
  the coordinator.
- Fleet ops: `ListNodesAsync` (connection, in-flight counts, labels, cordon state, last
  audited action), `CordonAsync`, `UncordonAsync`, `DeregisterAsync`, plus a client-side
  `DrainAsync` extension that cordons a node and polls until its in-flight count reaches
  zero — cordon → drain → uncordon from three lines of C#.
- Vector admin: `ListCollectionsAsync` (collections + replica placement),
  `GetCollectionAsync` (definition, placement, `underReplicated`, query stats; `null` on
  404), `CreateCollectionAsync(name, dimension, distance?)`, `DropCollectionAsync`, and
  `RebuildAsync` to force a heal-to-target replica re-push.
- `StreamAdminEventsAsync` tails `GET /api/admin/stream` (SSE) as an
  `IAsyncEnumerable<AdminEvent>`: fleet snapshots (on change and as a ~10s keepalive) and
  `vector.*` lifecycle events (collection created/dropped, replica assigned/lost, heal
  started/completed) with a monotonic sequence. An `AdminStreamOptions` overload
  reconnects with exponential backoff when the stream drops — auth failures are never
  retried.
- The admin `HttpClient` is registered without an overall timeout so the SSE stream can
  live indefinitely; `InferHubClientOptions.Timeout` still applies per non-streaming
  admin call and surfaces as a `TimeoutException`.
- `samples/FleetOps` — list nodes, cordon + drain + uncordon, walk the collections, and
  tail the admin stream in one console run.

## Install

```
dotnet add package InferHub.Client --version 0.6.0
```

## Minimal example

```csharp
var admin = provider.GetRequiredService<IInferHubAdminClient>();

var drained = await admin.DrainAsync("node-1");   // cordon + wait for in-flight == 0
// ...maintenance...
await admin.UncordonAsync("node-1");

await foreach (var ev in admin.StreamAdminEventsAsync(new AdminStreamOptions()))
{
    Console.WriteLine(ev.IsSnapshot
        ? $"fleet: {ev.Nodes!.Count} node(s)"
        : $"#{ev.Sequence} {ev.Event} {ev.Collection}");
}
```

Remote coordinators need an admin key (`Auth:AdminApiKeys`); loopback works without one
unless `Auth:RequireAuthForLoopback=true`. Hardening and 1.0 land next.

**Full Changelog**: https://github.com/Dev-Art-Solutions/InferHub.Client/compare/v0.5.0...v0.6.0
