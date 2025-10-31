using FluentAssertions;
using IdeaBranch.Infrastructure.Resilience;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http;
using Microsoft.Extensions.Logging;
using NUnit.Framework;

namespace IdeaBranch.UnitTests.Resilience;

/// <summary>
/// Unit tests for ServiceCollectionExtensions.
/// Tests dependency injection registration and HttpClientFactory integration.
/// </summary>
public class ServiceCollectionExtensionsTests
{
    [Test]
    public void AddResiliencePolicies_ShouldRegisterTelemetryEmitter()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();

        // Act
        services.AddResiliencePolicies();

        // Assert
        var provider = services.BuildServiceProvider();
        var telemetryEmitter = provider.GetService<ResilienceTelemetryEmitter>();
        telemetryEmitter.Should().NotBeNull();
    }

    [Test]
    public void AddResiliencePolicies_ShouldRegisterPolicyRegistry()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();

        // Act
        services.AddResiliencePolicies();

        // Assert
        var provider = services.BuildServiceProvider();
        var registry = provider.GetService<ResiliencePolicyRegistry>();
        registry.Should().NotBeNull();
    }

    [Test]
    public void AddResiliencePolicies_ShouldAllowConfigurationCallback()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        var configured = false;

        // Act
        services.AddResiliencePolicies(builder =>
        {
            configured = true;
        });

        // Assert
        configured.Should().BeTrue("configuration callback should be invoked");
    }

    [Test]
    public void AddStandardResiliencePolicy_ShouldConfigureNamedHttpClient()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddResiliencePolicies();
        services.AddHttpClient("TestClient");

        // Act
        services.AddHttpClient("TestClient")
            .AddStandardResiliencePolicy("TestClient");

        // Assert
        var provider = services.BuildServiceProvider();
        var factory = provider.GetRequiredService<IHttpClientFactory>();
        var client = factory.CreateClient("TestClient");
        client.Should().NotBeNull();
    }

    [Test]
    public void AddResiliencePolicy_ShouldConfigureCustomPolicy()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddResiliencePolicies();
        services.AddHttpClient("TestClient");

        // Act
        services.AddHttpClient("TestClient")
            .AddResiliencePolicy(sp =>
            {
                var registry = sp.GetRequiredService<ResiliencePolicyRegistry>();
                return registry.GetIdempotentHttpPolicy("CustomPolicy");
            });

        // Assert
        var provider = services.BuildServiceProvider();
        var factory = provider.GetRequiredService<IHttpClientFactory>();
        var client = factory.CreateClient("TestClient");
        client.Should().NotBeNull();
    }

    [Test]
    public void AddResiliencePolicies_ShouldRegisterServicesAsSingletons()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();

        // Act
        services.AddResiliencePolicies();

        // Assert
        var provider = services.BuildServiceProvider();
        var registry1 = provider.GetRequiredService<ResiliencePolicyRegistry>();
        var registry2 = provider.GetRequiredService<ResiliencePolicyRegistry>();
        
        registry1.Should().BeSameAs(registry2, "should be registered as singleton");
    }
}

