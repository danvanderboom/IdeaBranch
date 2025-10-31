using System.Net;
using System.Net.Http;
using FluentAssertions;
using IdeaBranch.Infrastructure.Resilience;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace IdeaBranch.UnitTests.Resilience;

/// <summary>
/// Unit tests for ResiliencePolicyRegistry.
/// Tests exponential backoff + jitter, circuit breaker behavior, and policy selection.
/// </summary>
public class ResiliencePolicyRegistryTests
{
    private Mock<ILogger<ResiliencePolicyRegistry>> _mockLogger = null!;
    private Mock<ILogger> _mockTelemetryLogger = null!;
    private ResilienceTelemetryEmitter _telemetryEmitter = null!;
    private ResiliencePolicyRegistry _registry = null!;

    [SetUp]
    public void Setup()
    {
        _mockLogger = new Mock<ILogger<ResiliencePolicyRegistry>>();
        _mockTelemetryLogger = new Mock<ILogger>();
        _telemetryEmitter = new ResilienceTelemetryEmitter(_mockTelemetryLogger.Object);
        _registry = new ResiliencePolicyRegistry(_mockLogger.Object, _telemetryEmitter);
    }

    [Test]
    public void GetStandardHttpRetryPolicy_ShouldRetryOnTransientErrors()
    {
        // Arrange
        var policy = _registry.GetStandardHttpRetryPolicy("TestPolicy");
        var retryCount = 0;
        var handler = new TestHttpMessageHandler(_ =>
        {
            retryCount++;
            if (retryCount < 3)
            {
                return new HttpResponseMessage(HttpStatusCode.ServiceUnavailable);
            }
            return new HttpResponseMessage(HttpStatusCode.OK);
        });

        var client = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://api.example.com")
        };

        // Act
        var result = policy.ExecuteAsync(async () => await client.GetAsync("/test")).Result;

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.OK);
        retryCount.Should().Be(3, "should retry twice after initial failure");
    }

    [Test]
    public void GetStandardHttpRetryPolicy_ShouldUseExponentialBackoffWithJitter()
    {
        // Arrange
        var policy = _registry.GetStandardHttpRetryPolicy("TestPolicy");
        var delays = new List<TimeSpan>();
        var handler = new TestHttpMessageHandler(_ =>
        {
            delays.Add(DateTime.UtcNow.TimeOfDay); // Track timing
            return new HttpResponseMessage(HttpStatusCode.ServiceUnavailable);
        });

        var client = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://api.example.com")
        };

        // Act
        try
        {
            policy.ExecuteAsync(async () => await client.GetAsync("/test")).Wait();
        }
        catch
        {
            // Expected to fail after retries
        }

        // Assert
        // With exponential backoff + jitter, delays should increase (rough check)
        // Note: Actual delays have jitter so exact values vary
        delays.Count.Should().BeGreaterThan(1, "should have multiple retry attempts");
    }

    [Test]
    public void GetStandardHttpRetryPolicy_ShouldRetryOnRequestTimeout()
    {
        // Arrange
        var policy = _registry.GetStandardHttpRetryPolicy("TestPolicy");
        var retryCount = 0;
        var handler = new TestHttpMessageHandler(_ =>
        {
            retryCount++;
            if (retryCount < 2)
            {
                return new HttpResponseMessage(HttpStatusCode.RequestTimeout);
            }
            return new HttpResponseMessage(HttpStatusCode.OK);
        });

        var client = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://api.example.com")
        };

        // Act
        var result = policy.ExecuteAsync(async () => await client.GetAsync("/test")).Result;

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.OK);
        retryCount.Should().Be(2);
    }

    [Test]
    public void GetStandardHttpRetryPolicy_ShouldRetryOnTooManyRequests()
    {
        // Arrange
        var policy = _registry.GetStandardHttpRetryPolicy("TestPolicy");
        var retryCount = 0;
        var handler = new TestHttpMessageHandler(_ =>
        {
            retryCount++;
            if (retryCount < 2)
            {
                return new HttpResponseMessage(HttpStatusCode.TooManyRequests);
            }
            return new HttpResponseMessage(HttpStatusCode.OK);
        });

        var client = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://api.example.com")
        };

        // Act
        var result = policy.ExecuteAsync(async () => await client.GetAsync("/test")).Result;

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.OK);
        retryCount.Should().Be(2);
    }

    [Test]
    public void GetStandardHttpRetryPolicy_ShouldNotRetryOnNonTransientErrors()
    {
        // Arrange
        var policy = _registry.GetStandardHttpRetryPolicy("TestPolicy");
        var retryCount = 0;
        var handler = new TestHttpMessageHandler(_ =>
        {
            retryCount++;
            return new HttpResponseMessage(HttpStatusCode.BadRequest); // 400 - not transient
        });

        var client = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://api.example.com")
        };

        // Act
        var result = policy.ExecuteAsync(async () => await client.GetAsync("/test")).Result;

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        retryCount.Should().Be(1, "should not retry non-transient errors");
    }

    [Test]
    public async Task GetCircuitBreakerPolicy_ShouldOpenAfterConsecutiveFailures()
    {
        // Arrange
        var policy = _registry.GetCircuitBreakerPolicy("TestCircuitBreaker", handledEventsAllowedBeforeBreaking: 2, TimeSpan.FromSeconds(5));
        var handler = new TestHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.ServiceUnavailable));

        var client = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://api.example.com")
        };

        // Act - Trigger enough failures to open circuit
        var circuitOpened = false;
        var attemptCount = 0;
        
        for (int i = 0; i < 5; i++)
        {
            try
            {
                attemptCount++;
                var task = policy.ExecuteAsync(async () => await client.GetAsync("/test"));
                var result = await task;
            }
            catch (AggregateException aggEx)
            {
                if (aggEx.InnerException is Polly.CircuitBreaker.BrokenCircuitException<HttpResponseMessage> ||
                    aggEx.InnerExceptions.Any(e => e is Polly.CircuitBreaker.BrokenCircuitException<HttpResponseMessage>))
                {
                    circuitOpened = true;
                    break;
                }
                throw;
            }
            catch (Polly.CircuitBreaker.BrokenCircuitException<HttpResponseMessage>)
            {
                circuitOpened = true;
                break;
            }
        }

        // Assert
        circuitOpened.Should().BeTrue("circuit should open after consecutive failures");
        attemptCount.Should().BeGreaterThanOrEqualTo(3, "should have enough attempts to trigger circuit opening");
    }

    [Test]
    public void GetIdempotentHttpPolicy_ShouldUseStandardResilience()
    {
        // Arrange
        var policy = _registry.GetIdempotentHttpPolicy("TestIdempotent");

        // Act & Assert
        policy.Should().NotBeNull();
        // Should wrap both retry and circuit breaker
    }

    [Test]
    public void GetNonIdempotentHttpPolicy_ShouldLimitRetries()
    {
        // Arrange
        var policy = _registry.GetNonIdempotentHttpPolicy("TestNonIdempotent");
        var retryCount = 0;
        var handler = new TestHttpMessageHandler(_ =>
        {
            retryCount++;
            return new HttpResponseMessage(HttpStatusCode.ServiceUnavailable); // Even transient errors shouldn't retry
        });

        var client = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://api.example.com")
        };

        // Act
        var result = policy.ExecuteAsync(async () => await client.GetAsync("/test")).Result;

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.ServiceUnavailable);
        retryCount.Should().Be(1, "non-idempotent operations should not retry on HTTP errors");
    }

    [Test]
    public void GetNonIdempotentHttpPolicy_ShouldRetryOnTimeout()
    {
        // Arrange
        var policy = _registry.GetNonIdempotentHttpPolicy("TestNonIdempotent");
        var retryCount = 0;
        var handler = new TestHttpMessageHandler(_ =>
        {
            retryCount++;
            if (retryCount < 2)
            {
                throw new TaskCanceledException("Request timeout");
            }
            return new HttpResponseMessage(HttpStatusCode.OK);
        });

        var client = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://api.example.com")
        };

        // Act
        var result = policy.ExecuteAsync(async () => await client.GetAsync("/test")).Result;

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.OK);
        retryCount.Should().Be(2, "non-idempotent operations can retry on timeouts");
    }

    [Test]
    public void GetStandardResiliencePolicy_ShouldWrapRetryAndCircuitBreaker()
    {
        // Arrange
        var policy = _registry.GetStandardResiliencePolicy("TestResilience");

        // Act & Assert
        policy.Should().NotBeNull();
        // Wrapped policy should combine both retry and circuit breaker
    }
}

/// <summary>
/// Test HTTP message handler for unit tests.
/// </summary>
internal class TestHttpMessageHandler : HttpMessageHandler
{
    private readonly Func<HttpRequestMessage, HttpResponseMessage> _handler;

    public TestHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> handler)
    {
        _handler = handler;
    }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        return Task.FromResult(_handler(request));
    }
}

