using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Extensions.Http;

namespace IdeaBranch.Infrastructure.Resilience;

/// <summary>
/// Extension methods for registering resilience policies with dependency injection.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers resilience policy registry and configures HttpClientFactory with standard policies.
    /// </summary>
    public static IServiceCollection AddResiliencePolicies(
        this IServiceCollection services,
        Action<ResiliencePolicyRegistryBuilder>? configure = null)
    {
        // Register telemetry emitter
        services.AddSingleton<ResilienceTelemetryEmitter>(sp =>
        {
            var logger = sp.GetRequiredService<ILogger<ResilienceTelemetryEmitter>>();
            return new ResilienceTelemetryEmitter(logger);
        });

        // Register policy registry
        services.AddSingleton<ResiliencePolicyRegistry>(sp =>
        {
            var logger = sp.GetRequiredService<ILogger<ResiliencePolicyRegistry>>();
            var telemetry = sp.GetRequiredService<ResilienceTelemetryEmitter>();
            return new ResiliencePolicyRegistry(logger, telemetry);
        });

        // Note: Default HttpClientFactory policies are applied per named client
        // Use AddStandardResiliencePolicy() when configuring named clients

        // Allow additional configuration
        if (configure != null)
        {
            var builder = new ResiliencePolicyRegistryBuilder(services);
            configure(builder);
        }

        return services;
    }

    /// <summary>
    /// Configures a named HttpClient with a specific resilience policy.
    /// </summary>
    public static IHttpClientBuilder AddResiliencePolicy(
        this IHttpClientBuilder builder,
        Func<IServiceProvider, IAsyncPolicy<HttpResponseMessage>> policyFactory)
    {
        return builder.AddPolicyHandler((sp, request) => policyFactory(sp));
    }

    /// <summary>
    /// Configures a named HttpClient with the standard resilience policy.
    /// </summary>
    public static IHttpClientBuilder AddStandardResiliencePolicy(
        this IHttpClientBuilder builder,
        string policyName = "StandardResilience")
    {
        return builder.AddPolicyHandler((sp, request) =>
        {
            var registry = sp.GetRequiredService<ResiliencePolicyRegistry>();
            var method = request.Method;
            
            return method switch
            {
                HttpMethod m when m == HttpMethod.Get ||
                                  m == HttpMethod.Put ||
                                  m == HttpMethod.Delete ||
                                  m == HttpMethod.Patch ||
                                  m == HttpMethod.Head ||
                                  m == HttpMethod.Options
                    => registry.GetIdempotentHttpPolicy($"{policyName}_Idempotent"),
                _ => registry.GetNonIdempotentHttpPolicy($"{policyName}_NonIdempotent")
            };
        });
    }
}

/// <summary>
/// Builder for configuring resilience policy registry.
/// </summary>
public sealed class ResiliencePolicyRegistryBuilder
{
    public IServiceCollection Services { get; }

    public ResiliencePolicyRegistryBuilder(IServiceCollection services)
    {
        Services = services;
    }
}

