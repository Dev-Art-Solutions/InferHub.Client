namespace InferHub.Client.Configuration;

/// <summary>
/// Configuration for <see cref="IInferHubClient"/>.
/// </summary>
public sealed class InferHubClientOptions
{
    /// <summary>Default coordinator base URL used when none is configured.</summary>
    public const string DefaultBaseAddress = "http://localhost:5080/";

    /// <summary>
    /// Base URL of the InferHub coordinator (e.g. <c>http://localhost:5080/</c>).
    /// </summary>
    public Uri BaseAddress { get; set; } = new(DefaultBaseAddress);

    /// <summary>
    /// Client API key sent as <c>Authorization: Bearer &lt;key&gt;</c>. Not required on
    /// loopback calls unless the coordinator sets <c>Auth:RequireAuthForLoopback=true</c>.
    /// </summary>
    public string? ApiKey { get; set; }

    /// <summary>
    /// Admin API key, sent as the bearer token by <see cref="IInferHubAdminClient"/> only.
    /// The coordinator validates admin routes against a separate key set
    /// (<c>Auth:AdminApiKeys</c>); a client key alone never surfaces admin methods.
    /// </summary>
    public string? AdminApiKey { get; set; }

    /// <summary>
    /// HTTP timeout applied to the underlying <see cref="HttpClient"/>. Defaults to
    /// 100 seconds — inference calls can take a while, tune to your workload.
    /// </summary>
    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(100);
}
