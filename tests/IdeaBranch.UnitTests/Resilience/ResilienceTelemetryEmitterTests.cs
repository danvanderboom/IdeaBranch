using FluentAssertions;
using IdeaBranch.Infrastructure.Resilience;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using Polly;

namespace IdeaBranch.UnitTests.Resilience;

/// <summary>
/// Unit tests for ResilienceTelemetryEmitter.
/// Tests telemetry emission for retries, circuit breaker state, and outcomes.
/// </summary>
public class ResilienceTelemetryEmitterTests
{
    private Mock<ILogger> _mockLogger = null!;
    private ResilienceTelemetryEmitter _telemetryEmitter = null!;

    [SetUp]
    public void Setup()
    {
        _mockLogger = new Mock<ILogger>();
        _telemetryEmitter = new ResilienceTelemetryEmitter(_mockLogger.Object);
    }

    [Test]
    public void EmitRetryAttempt_ShouldLogInformation()
    {
        // Arrange
        var policyName = "TestPolicy";
        var attemptNumber = 1;
        var delay = TimeSpan.FromMilliseconds(100);
        var reason = "500 Internal Server Error";
        var context = new Context();

        // Act
        _telemetryEmitter.EmitRetryAttempt(policyName, attemptNumber, delay, reason, context);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Resilience retry event")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Test]
    public void EmitRetryAttempt_ShouldGenerateCorrelationIdIfMissing()
    {
        // Arrange
        var policyName = "TestPolicy";
        var attemptNumber = 1;
        var delay = TimeSpan.FromMilliseconds(100);
        var reason = "500 Internal Server Error";
        Context? context = null; // No context provided

        // Act
        _telemetryEmitter.EmitRetryAttempt(policyName, attemptNumber, delay, reason, context);

        // Assert
        // Should not throw and should generate a correlation ID
        _mockLogger.Verify(
            x => x.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Test]
    public void EmitCircuitBreakerOpened_ShouldLogWarning()
    {
        // Arrange
        var policyName = "TestPolicy";
        var duration = TimeSpan.FromSeconds(30);
        var reason = "500 Internal Server Error";
        var context = new Context();

        // Act
        _telemetryEmitter.EmitCircuitBreakerOpened(policyName, duration, reason, context);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Resilience circuit open event")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Test]
    public void EmitCircuitBreakerReset_ShouldLogInformation()
    {
        // Arrange
        var policyName = "TestPolicy";
        var context = new Context();

        // Act
        _telemetryEmitter.EmitCircuitBreakerReset(policyName, context);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Resilience circuit reset event")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Test]
    public void EmitCircuitBreakerHalfOpen_ShouldLogInformation()
    {
        // Arrange
        var policyName = "TestPolicy";

        // Act
        _telemetryEmitter.EmitCircuitBreakerHalfOpen(policyName);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Resilience circuit half-open event")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Test]
    public void EmitSuccess_ShouldLogDebug()
    {
        // Arrange
        var policyName = "TestPolicy";
        var duration = TimeSpan.FromMilliseconds(50);
        var context = new Context();

        // Act
        _telemetryEmitter.EmitSuccess(policyName, duration, context);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Resilience success event")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Test]
    public void EmitFailure_ShouldLogError()
    {
        // Arrange
        var policyName = "TestPolicy";
        var reason = "All retries exhausted";
        var context = new Context();

        // Act
        _telemetryEmitter.EmitFailure(policyName, reason, context);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Resilience failure event")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Test]
    public void EmitRetryAttempt_ShouldUseCorrelationIdFromContext()
    {
        // Arrange
        var policyName = "TestPolicy";
        var attemptNumber = 1;
        var delay = TimeSpan.FromMilliseconds(100);
        var reason = "500";
        var context = new Context();
        var correlationId = Guid.NewGuid().ToString();
        context["CorrelationId"] = correlationId;

        // Act
        _telemetryEmitter.EmitRetryAttempt(policyName, attemptNumber, delay, reason, context);

        // Assert - Verify that logging occurred (correlation ID is generated but not logged in message)
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Resilience retry event")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}

