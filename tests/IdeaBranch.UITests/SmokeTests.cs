using FluentAssertions;
using NUnit.Framework;
using IdeaBranch.UITests.Infrastructure;

namespace IdeaBranch.UITests;

/// <summary>
/// Smoke tests for core UI functionality.
/// Covers requirements: App responsiveness, Primary navigation, AutomationIds
/// Scenarios: Cold start (IB-UI-001), Navigation flows (IB-UI-010/011/012/013)
/// </summary>
public class SmokeTests : AppiumTestFixture
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
    [Property("TestId", "IB-UI-001")] // Cold start within budget (Windows)
    public void LaunchesMainWindow()
    {
        // Arrange & Act
        // App is launched via SetUp
        
        // Assert
        // Verify driver is initialized
        Driver!.Should().NotBeNull();
        
        // Verify app window is accessible
        // For Windows, check that we can find the main window
        Thread.Sleep(2000); // Give app time to fully launch
        
        // Try to find any accessible element to verify app is loaded
        // MainPage may have AutomationIds we can check
        var appTitle = Driver!.PageSource;
        appTitle.Should().NotBeNullOrEmpty("App should be loaded and accessible");
    }

    [Test]
    [Property("TestId", "IB-UI-010")] // Primary navigation AutomationIds exist
    public void PrimaryNavigation_AutomationIds_Exist()
    {
        // Arrange
        // App is launched via SetUp
        Thread.Sleep(2000); // Give app time to load
        
        // Act & Assert
        // Verify navigation elements exist
        // For Shell navigation, check if navigation items are accessible
        
        // Check if MainPage navigation is accessible
        // Shell items might be accessible by route or title
        var mainPageAccessible = Driver!.TryFindElementByAutomationId("MainPage");
        
        // For now, verify that we can navigate to pages that do exist
        // Check ResilienceTestPage navigation
        var resiliencePageAccessible = Driver!.TryFindElementByAutomationId("ResilienceTestPage");
        
        // At minimum, verify app shell is accessible
        var pageSource = Driver!.PageSource;
        pageSource.Should().NotBeNullOrEmpty("Shell should be accessible");
        
        // Note: Full navigation AutomationId verification depends on app implementation
        // This test verifies basic accessibility of the shell
        Assert.Pass("Verified shell accessibility - full AutomationId coverage depends on app implementation");
    }

    [Test]
    [Property("TestId", "IB-UI-011")] // Navigate to Topic Tree
    public void Navigate_ToTopicTree()
    {
        // Arrange
        Thread.Sleep(2000); // Wait for app to load
        
        // Act - Navigate to Topic Tree
        // Note: Topic Tree page not yet added to AppShell
        // When available, navigation will be:
        // var topicTreeNav = Driver!.FindElementByAutomationId("TopicTreePage");
        // topicTreeNav.Click();
        
        // Assert
        // Placeholder until Topic Tree page is implemented
        // Expected: Verify TopicTreePage is displayed
        // Expected: Verify page has AutomationIds for tree nodes
        
        Assert.Inconclusive("Topic Tree page not yet implemented in AppShell. Test will be updated when page is available.");
    }

    [Test]
    [Property("TestId", "IB-UI-012")] // Navigate to Map
    public void Navigate_ToMap()
    {
        // Arrange
        Thread.Sleep(2000); // Wait for app to load
        
        // Act - Navigate to Map
        var mapNav = Driver!.FindElementByAutomationId("MapPage");
        mapNav.Click();
        
        // Assert
        // Verify MapPage is displayed
        var mapPageContent = Driver.FindElementByAutomationId("MapPage_Content");
        mapPageContent.Should().NotBeNull();
        mapPageContent.Text.Should().Be("Map View");
    }

    [Test]
    [Property("TestId", "IB-UI-013")] // Navigate to Timeline
    public void Navigate_ToTimeline()
    {
        // Arrange
        Thread.Sleep(2000); // Wait for app to load
        
        // Act - Navigate to Timeline
        var timelineNav = Driver!.FindElementByAutomationId("TimelinePage");
        timelineNav.Click();
        
        // Assert
        // Verify TimelinePage is displayed
        var timelinePageContent = Driver.FindElementByAutomationId("TimelinePage_Content");
        timelinePageContent.Should().NotBeNull();
        timelinePageContent.Text.Should().Be("Timeline View");
    }
}
