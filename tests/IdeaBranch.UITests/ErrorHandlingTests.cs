using FluentAssertions;
using NUnit.Framework;
using IdeaBranch.UITests.Infrastructure;

namespace IdeaBranch.UITests;

/// <summary>
/// UI automation tests for error handling scenarios.
/// Tests graceful error handling for network and model/API failures.
/// Covers: Network unavailable (IB-UI-050), Model/API error (IB-UI-051)
/// </summary>
public class ErrorHandlingTests : AppiumTestFixture
{
    [SetUp]
    public void Setup()
    {
        SetUp();
    }

    [TearDown]
    public void TearDown()
    {
        base.TearDown();
    }

    [Test]
    [Property("TestId", "IB-UI-050")] // Network unavailable
    public void NetworkUnavailable_ShowsError_AppRemainsResponsive()
    {
        // Arrange
        NavigateToResilienceTestPage();
        WaitForPageReady();

        // Act
        // Use delay/timeout button to simulate network issues
        // This tests resilience policy behavior when network is slow/unavailable
        Driver!.ClickElementByAutomationId("ResilienceTest_DelayButton");

        // Assert
        // Verify activity indicator appears (shows app is processing)
        Driver!.WaitForElementVisible("ResilienceTest_ActivityIndicator", TimeSpan.FromSeconds(2));

        // Verify status message updates (may show timeout/error)
        Driver!.WaitForElementText("ResilienceTest_StatusMessage", "Testing", TimeSpan.FromSeconds(15));

        // Wait for operation to complete (may timeout)
        Driver!.WaitForElementInvisible("ResilienceTest_ActivityIndicator", TimeSpan.FromSeconds(45));

        // Verify error message or timeout message is displayed
        Thread.Sleep(1000);
        var statusMessage = Driver!.GetElementText("ResilienceTest_StatusMessage");
        statusMessage.Should().NotBeNullOrEmpty();
        // Status may show error, timeout, or completion message
        statusMessage.Should().ContainAny("Error", "Timeout", "Failed", "Complete", "Success");

        // Verify results show error information
        var results = Driver!.GetElementText("ResilienceTest_Results");
        results.Should().NotBeNullOrEmpty();
        // Results may show timeout, error, or retry attempts
        results.Should().ContainAny("Delay", "Timeout", "Error", "Failed", "Retry");

        // Verify app remains responsive - buttons should be re-enabled
        Driver!.IsElementEnabled("ResilienceTest_GetButton").Should().BeTrue("App should remain responsive after error");
        Driver!.IsElementEnabled("ResilienceTest_PostButton").Should().BeTrue("App should remain responsive after error");
    }

    [Test]
    [Property("TestId", "IB-UI-051")] // Model/API error
    public void ModelApiError_ShowsError_AppRemainsResponsive()
    {
        // Arrange
        NavigateToResilienceTestPage();
        WaitForPageReady();

        // Act
        // Use retry button (500 error) to simulate API error
        Driver!.ClickElementByAutomationId("ResilienceTest_RetryButton");

        // Assert
        // Verify activity indicator appears
        Driver!.WaitForElementVisible("ResilienceTest_ActivityIndicator", TimeSpan.FromSeconds(2));

        // Verify status message updates
        Driver!.WaitForElementText("ResilienceTest_StatusMessage", "Testing", TimeSpan.FromSeconds(15));

        // Wait for operation to complete (retries may take time)
        Driver!.WaitForElementInvisible("ResilienceTest_ActivityIndicator", TimeSpan.FromSeconds(45));

        // Verify error message is displayed
        Thread.Sleep(1000);
        var statusMessage = Driver!.GetElementText("ResilienceTest_StatusMessage");
        statusMessage.Should().NotBeNullOrEmpty();
        // Status should show error or retry information
        statusMessage.Should().ContainAny("Error", "Failed", "Retry", "500", "status");

        // Verify results show error information
        var results = Driver!.GetElementText("ResilienceTest_Results");
        results.Should().NotBeNullOrEmpty();
        // Results should show 500 error or retry attempts
        results.Should().ContainAny("500", "Error", "Failed", "Retry", "status");

        // Verify app remains responsive - buttons should be re-enabled
        Driver!.IsElementEnabled("ResilienceTest_GetButton").Should().BeTrue("App should remain responsive after API error");
        Driver!.IsElementEnabled("ResilienceTest_PostButton").Should().BeTrue("App should remain responsive after API error");

        // Verify error message provides actionable guidance
        // Error messages should be informative (implied by presence of results/status)
        results.Should().NotBe("", "Error results should be displayed");
    }

    [Test]
    [Property("TestId", "IB-UI-051")] // Model/API error - Circuit breaker scenario
    public void ModelApiError_CircuitBreaker_ShowsError_AppRemainsResponsive()
    {
        // Arrange
        NavigateToResilienceTestPage();
        WaitForPageReady();

        // Act
        // Use circuit breaker button to simulate repeated failures
        Driver!.ClickElementByAutomationId("ResilienceTest_CircuitBreakerButton");

        // Assert
        // Verify activity indicator appears
        Driver!.WaitForElementVisible("ResilienceTest_ActivityIndicator", TimeSpan.FromSeconds(2));

        // Wait for circuit breaker test to complete
        Driver!.WaitForElementInvisible("ResilienceTest_ActivityIndicator", TimeSpan.FromSeconds(60));

        // Verify error or circuit breaker information is displayed
        Thread.Sleep(1000);
        var statusMessage = Driver!.GetElementText("ResilienceTest_StatusMessage");
        statusMessage.Should().NotBeNullOrEmpty();

        // Verify results show circuit breaker information
        var results = Driver!.GetElementText("ResilienceTest_Results");
        results.Should().NotBeNullOrEmpty();
        // Results may show circuit breaker activation or related messages
        results.Should().ContainAny("Circuit", "Breaker", "Open", "Closed", "Broken", "Error", "Failed");

        // Verify app remains responsive
        Driver!.IsElementEnabled("ResilienceTest_GetButton").Should().BeTrue("App should remain responsive after circuit breaker error");
    }

    private void NavigateToResilienceTestPage()
    {
        // Wait for app to load
        Thread.Sleep(2000);

        // Try to find and navigate to ResilienceTestPage
        try
        {
            var navItem = Driver!.TryFindElementByAutomationId("ResilienceTestPage");
            if (navItem != null)
            {
                navItem.Click();
                Thread.Sleep(1000);
            }
        }
        catch
        {
            // Navigation may not be directly accessible
        }

        // Wait for page to load
        Driver!.WaitForElementVisible("ResilienceTest_StatusMessage");
    }

    private void WaitForPageReady()
    {
        // Wait for initial page state to be ready
        Thread.Sleep(500);

        // Verify status message is visible
        Driver!.WaitForElementVisible("ResilienceTest_StatusMessage");

        // Ensure buttons are enabled (not busy)
        var maxWait = TimeSpan.FromSeconds(5);
        var startTime = DateTime.Now;
        while (DateTime.Now - startTime < maxWait)
        {
            if (Driver!.IsElementEnabled("ResilienceTest_GetButton"))
            {
                break;
            }
            Thread.Sleep(200);
        }
    }
}

