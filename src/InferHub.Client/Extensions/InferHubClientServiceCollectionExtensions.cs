using InferHub.Client.Configuration;
using InferHub.Client.Http;
using Microsoft.Extensions.DependencyInjection;

namespace InferHub.Client.Extensions;

/// <summary>DI wiring for <see cref="IInferHubClient"/>.</summary>
public static class InferHubClientServiceCollectionExtensions
{
    /// <summary>
    /// Register <see cref="IInferHubClient"/> with a typed <see cref="HttpClient"/> and a
    /// bearer-auth <see cref="DelegatingHandler"/>.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configure">Configure the options (base address, keys, timeout).</param>
    /// <returns>The same <paramref name="services"/> for chaining.</returns>
    public static IServiceCollection AddInferHubClient(this IServiceCollection services, Action<InferHubClientOptions> configure)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configure);

        var options = new InferHubClientOptions();
        configure(options);

        if (options.BaseAddress is null)
        {
            throw new ArgumentException($"{nameof(InferHubClientOptions.BaseAddress)} is required.", nameof(configure));
        }

        services.AddSingleton(options);
        services.AddTransient<BearerAuthorizationHandler>(_ => new BearerAuthorizationHandler(options));

        services.AddHttpClient<IInferHubClient, InferHubClient>(client =>
        {
            client.BaseAddress = EnsureTrailingSlash(options.BaseAddress);
            client.Timeout = options.Timeout;
        })
        .AddHttpMessageHandler<BearerAuthorizationHandler>();

        return services;
    }

    private static Uri EnsureTrailingSlash(Uri uri)
    {
        var raw = uri.ToString();
        return raw.EndsWith('/') ? uri : new Uri(raw + "/", UriKind.Absolute);
    }
}
