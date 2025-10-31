using FluentAssertions;
using NUnit.Framework;
using IdeaBranch.UITests.Infrastructure;
using OpenQA.Selenium;

namespace IdeaBranch.UITests;

/// <summary>
/// UI automation tests for ResilienceTestPage.
/// Tests resilience policy behavior via UI interactions.
/// Covers: Navigation, button interactions, status messages, telemetry observation.
/// </summary>
public class ResilienceTests : AppiumTestFixture
{
    [SetUp]
    public void Setup()
    {
        SetUp();
    }

    [TearDown]
    protected override void TearDown()
    {
        base.TearDown();
    }

    [Test]
    [Property("TestId", "RESILIENCE-001")] // Navigate to Resilience Test page
    public void Navigate_ToResilienceTestPage()
    {
        // Arrange
        // App is already launched via SetUp
        
        // Act - Navigate to ResilienceTestPage using Shell navigation
        // For Windows, we can navigate via flyout or directly by finding the navigation item
        // Try to find and click the "Resilience Test" navigation item
        try
        {
            // Wait for app to load
            Thread.Sleep(2000);
            
            // For Windows/UIA3, navigation items might be accessible via AutomationId or name
            // Try finding the shell content item or flyout item
            var resilienceNavItem = Driver!.FindElementByAutomationId("ResilienceTestPage");
            resilienceNavItem.Click();
        }
        catch
        {
            // Alternative: Navigate programmatically if AutomationId not found
            // For now, assume navigation can be done via app code or we're already on the page
        }
        
        // Assert
        // Verify ResilienceTestPage is displayed by checking for a unique element
        Driver!.WaitForElementVisible("ResilienceTest_StatusMessage");
        
        // Verify page title label is visible
        var statusMessage = Driver!.GetElementText("ResilienceTest_StatusMessage");
        statusMessage.Should().NotBeNullOrEmpty();
        
        // Verify page title contains expected content
        statusMessage.Should().ContainAny("Ready", "Resilience");
    }

    [Test]
    [Property("TestId", "RESILIENCE-002")] // Test GET button exists and is enabled
    public void ResilienceTestPage_TestGetButton_Exists()
    {
        // Arrange
        NavigateToResilienceTestPage();
        
        // Act
        var getButton = Driver!.FindElementByAutomationId("ResilienceTest_GetButton");
        
        // Assert
        getButton.Should().NotBeNull();
        getButton.Displayed.Should().BeTrue();
        
        // Verify button is enabled when not busy
        // Wait a moment to ensure initial state is ready
        Thread.Sleep(500);
        Driver!.IsElementEnabled("ResilienceTest_GetButton").Should().BeTrue();
        
        // Verify button text
        getButton.Text.Should().Contain("Test GET");
    }

    [Test]
    [Property("TestId", "RESILIENCE-003")] // Test GET button click triggers API call
    public void ResilienceTestPage_TestGetButton_Click_TriggersApiCall()
    {
        // Arrange
        NavigateToResilienceTestPage();
        
        // Get initial status
        var initialStatus = Driver!.GetElementText("ResilienceTest_StatusMessage");
        initialStatus.Should().NotBeNullOrEmpty();
        
        // Act
        Driver!.ClickElementByAutomationId("ResilienceTest_GetButton");
        
        // Assert
        // Verify activity indicator appears during request
        Driver!.WaitForElementVisible("ResilienceTest_ActivityIndicator", TimeSpan.FromSeconds(2));
        
        // Verify status message changes
        Driver!.WaitForElementText("ResilienceTest_StatusMessage", "Testing", TimeSpan.FromSeconds(15));
        var statusAfterClick = Driver!.GetElementText("ResilienceTest_StatusMessage");
        statusAfterClick.Should().ContainAny("Testing", "Success", "GET");
        
        // Wait for operation to complete
        Driver!.WaitForElementInvisible("ResilienceTest_ActivityIndicator", TimeSpan.FromSeconds(30));
        
        // Verify results are displayed
        Thread.Sleep(1000); // Give a moment for results to update
        var results = Driver!.GetElementText("ResilienceTest_Results");
        results.Should().NotBeNullOrEmpty();
        results.Should().ContainAny("GET", "httpbin", "Success", "200");
    }

    [Test]
    [Property("TestId", "RESILIENCE-004")] // Test POST button click with limited retry
    public void ResilienceTestPage_TestPostButton_Click_TriggersApiCall()
    {
        // Arrange
        NavigateToResilienceTestPage();
        WaitForPageReady();
        
        // Act
        Driver!.ClickElementByAutomationId("ResilienceTest_PostButton");
        
        // Assert
        // Verify activity indicator appears
        Driver!.WaitForElementVisible("ResilienceTest_ActivityIndicator", TimeSpan.FromSeconds(2));
        
        // Verify status message updates
        Driver!.WaitForElementText("ResilienceTest_StatusMessage", "Testing", TimeSpan.FromSeconds(15));
        
        // Wait for operation to complete
        Driver!.WaitForElementInvisible("ResilienceTest_ActivityIndicator", TimeSpan.FromSeconds(30));
        
        // Verify results are displayed
        Thread.Sleep(1000);
        var results = Driver!.GetElementText("ResilienceTest_Results");
        results.Should().NotBeNullOrEmpty();
        results.Should().ContainAny("POST", "httpbin", "Success", "200");
    }

    [Test]
    [Property("TestId", "RESILIENCE-005")] // Test retry button with 500 error
    public void ResilienceTestPage_TestRetryButton_Click_ShowsRetryBehavior()
    {
        // Arrange
        NavigateToResilienceTestPage();
        WaitForPageReady();
        
        // Act
        Driver!.ClickElementByAutomationId("ResilienceTest_RetryButton");
        
        // Assert
        // Verify activity indicator appears
        Driver!.WaitForElementVisible("ResilienceTest_ActivityIndicator", TimeSpan.FromSeconds(2));
        
        // Verify status message updates (may show retry attempts)
        Driver!.WaitForElementText("ResilienceTest_StatusMessage", "Testing", TimeSpan.FromSeconds(15));
        
        // Wait for operation to complete (retries may take longer)
        Driver!.WaitForElementInvisible("ResilienceTest_ActivityIndicator", TimeSpan.FromSeconds(45));
        
        // Verify results show retry attempts or failure
        Thread.Sleep(1000);
        var results = Driver!.GetElementText("ResilienceTest_Results");
        results.Should().NotBeNullOrEmpty();
        // Results may show retry attempts, 500 status, or failure message
        results.Should().ContainAny("Retry", "500", "Error", "Failed", "status");
    }

    [Test]
    [Property("TestId", "RESILIENCE-006")] // Test circuit breaker button
    public void ResilienceTestPage_TestCircuitBreakerButton_Click_ShowsCircuitBreakerBehavior()
    {
        // Arrange
        NavigateToResilienceTestPage();
        WaitForPageReady();
        
        // Act
        Driver!.ClickElementByAutomationId("ResilienceTest_CircuitBreakerButton");
        
        // Assert
        // Verify activity indicator appears
        Driver!.WaitForElementVisible("ResilienceTest_ActivityIndicator", TimeSpan.FromSeconds(2));
        
        // Wait for operation to complete (circuit breaker test may take time)
        Driver!.WaitForElementInvisible("ResilienceTest_ActivityIndicator", TimeSpan.FromSeconds(60));
        
        // Verify results show circuit breaker information
        Thread.Sleep(1000);
        var results = Driver!.GetElementText("ResilienceTest_Results");
        results.Should().NotBeNullOrEmpty();
        // Results may show circuit breaker activation or related messages
        results.Should().ContainAny("Circuit", "Breaker", "Open", "Closed", "Broken");
        
        // Verify status message updated
        var status = Driver!.GetElementText("ResilienceTest_StatusMessage");
        status.Should().NotBeNullOrEmpty();
    }

    [Test]
    [Property("TestId", "RESILIENCE-007")] // Test delay/timeout button
    public void ResilienceTestPage_TestDelayButton_Click_ShowsTimeoutBehavior()
    {
        // Arrange
        NavigateToResilienceTestPage();
        WaitForPageReady();
        
        // Act
        Driver!.ClickElementByAutomationId("ResilienceTest_DelayButton");
        
        // Assert
        // Verify activity indicator appears
        Driver!.WaitForElementVisible("ResilienceTest_ActivityIndicator", TimeSpan.FromSeconds(2));
        
        // Verify status message updates
        Driver!.WaitForElementText("ResilienceTest_StatusMessage", "Testing", TimeSpan.FromSeconds(15));
        
        // Wait for operation to complete (may timeout)
        Driver!.WaitForElementInvisible("ResilienceTest_ActivityIndicator", TimeSpan.FromSeconds(45));
        
        // Verify results show timeout or completion
        Thread.Sleep(1000);
        var results = Driver!.GetElementText("ResilienceTest_Results");
        results.Should().NotBeNullOrEmpty();
        // Results may show timeout, delay completion, or related messages
        results.Should().ContainAny("Delay", "Timeout", "Success", "200");
    }

    [Test]
    [Property("TestId", "RESILIENCE-008")] // Verify status message updates during operations
    public void ResilienceTestPage_StatusMessage_UpdatesDuringOperations()
    {
        // Arrange
        NavigateToResilienceTestPage();
        WaitForPageReady();
        
        // Get initial status
        var initialStatus = Driver!.GetElementText("ResilienceTest_StatusMessage");
        initialStatus.Should().NotBeNullOrEmpty();
        
        // Act
        Driver!.ClickElementByAutomationId("ResilienceTest_GetButton");
        
        // Assert
        // Verify status message changes from initial state
        Driver!.WaitForElementText("ResilienceTest_StatusMessage", "Testing", TimeSpan.FromSeconds(10));
        var statusDuringOperation = Driver!.GetElementText("ResilienceTest_StatusMessage");
        statusDuringOperation.Should().NotBe(initialStatus);
        statusDuringOperation.Should().ContainAny("Testing", "GET", "Request");
        
        // Wait for completion
        Driver!.WaitForElementInvisible("ResilienceTest_ActivityIndicator", TimeSpan.FromSeconds(30));
        
        // Verify status message updates on completion
        Thread.Sleep(1000);
        var finalStatus = Driver!.GetElementText("ResilienceTest_StatusMessage");
        finalStatus.Should().NotBeNullOrEmpty();
        // Status should show completion (success or failure)
        finalStatus.Should().ContainAny("Success", "Complete", "Done", "Failed", "Error");
    }

    [Test]
    [Property("TestId", "RESILIENCE-009")] // Verify results display updates
    public void ResilienceTestPage_ResultsDisplay_UpdatesWithResults()
    {
        // Arrange
        NavigateToResilienceTestPage();
        WaitForPageReady();
        
        // Get initial results (may be empty or have previous results)
        var initialResults = Driver!.TryFindElementByAutomationId("ResilienceTest_Results")?.Text ?? string.Empty;
        
        // Act
        Driver!.ClickElementByAutomationId("ResilienceTest_GetButton");
        
        // Wait for operation to complete
        Driver!.WaitForElementInvisible("ResilienceTest_ActivityIndicator", TimeSpan.FromSeconds(30));
        Thread.Sleep(1000); // Give time for results to update
        
        // Assert
        var finalResults = Driver!.GetElementText("ResilienceTest_Results");
        finalResults.Should().NotBeNullOrEmpty();
        
        // Verify results show operation type
        finalResults.Should().ContainAny("GET", "Testing", "Request");
        
        // Verify results show outcome (success or failure indicators)
        finalResults.Should().ContainAny("Success", "200", "Failed", "Error", "Complete");
        
        // Results should have changed (unless initial was already populated)
        if (!string.IsNullOrWhiteSpace(initialResults))
        {
            finalResults.Should().NotBe(initialResults);
        }
    }

    [Test]
    [Property("TestId", "RESILIENCE-010")] // Verify buttons disabled during busy state
    public void ResilienceTestPage_Buttons_DisabledDuringBusy()
    {
        // Arrange
        NavigateToResilienceTestPage();
        WaitForPageReady();
        
        // Verify buttons are initially enabled
        Driver!.IsElementEnabled("ResilienceTest_GetButton").Should().BeTrue();
        Driver!.IsElementEnabled("ResilienceTest_PostButton").Should().BeTrue();
        
        // Act
        Driver!.ClickElementByAutomationId("ResilienceTest_GetButton");
        
        // Assert
        // Verify activity indicator is visible
        Driver!.WaitForElementVisible("ResilienceTest_ActivityIndicator", TimeSpan.FromSeconds(2));
        
        // Verify buttons are disabled during operation
        // Note: This may be timing-sensitive, so check quickly after click
        Thread.Sleep(500);
        Driver!.IsElementEnabled("ResilienceTest_GetButton").Should().BeFalse("Button should be disabled during busy state");
        Driver!.IsElementEnabled("ResilienceTest_PostButton").Should().BeFalse("Button should be disabled during busy state");
        
        // Wait for operation to complete
        Driver!.WaitForElementInvisible("ResilienceTest_ActivityIndicator", TimeSpan.FromSeconds(30));
        Thread.Sleep(500);
        
        // Verify buttons are re-enabled after operation completes
        Driver!.IsElementEnabled("ResilienceTest_GetButton").Should().BeTrue("Button should be re-enabled after operation completes");
        Driver!.IsElementEnabled("ResilienceTest_PostButton").Should().BeTrue("Button should be re-enabled after operation completes");
    }
    
    private void NavigateToResilienceTestPage()
    {
        // Wait for app to load
        Thread.Sleep(2000);
        
        // Try to find and navigate to ResilienceTestPage
        // For Windows/UIA3 with Shell, navigation items may be accessible
        try
        {
            // Try finding navigation item by AutomationId if available
            var navItem = Driver!.TryFindElementByAutomationId("ResilienceTestPage");
            if (navItem != null)
            {
                navItem.Click();
                Thread.Sleep(1000);
            }
            else
            {
                // Alternative: Navigate via app code or assume we're already on the page
                // For now, we'll assume navigation happens automatically or via app logic
            }
        }
        catch
        {
            // Navigation may not be directly accessible, assume we're on the page or navigate programmatically
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

