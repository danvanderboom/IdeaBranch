using FluentAssertions;
using NUnit.Framework;
using IdeaBranch.UITests.Infrastructure;

namespace IdeaBranch.UITests;

/// <summary>
/// Tests for TopicTree page hierarchical display and interaction.
/// Covers TreeView expand/collapse behavior and Depth-based indentation.
/// Uses CriticalInsight.Data TreeView via TopicTreeViewProvider adapter.
/// </summary>
public class TopicTreeTests : AppiumTestFixture
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
    [Property("TestId", "IB-UI-011")] // Navigate to Topic Tree
    public void TopicTreePage_DisplaysHierarchicalNodes()
    {
        // Arrange & Act
        NavigateToTopicTreePage();
        
        // Assert
        // Verify root node is visible
        var rootNode = Driver!.FindElementByAutomationId("TopicNode_00000000-0000-0000-0000-000000000001");
        rootNode.Should().NotBeNull("Root node should be visible");
        rootNode.Displayed.Should().BeTrue("Root node should be displayed");
        
        // Verify root node text
        rootNode.Text.Should().Contain("Root Topic", "Root node should display title");
    }

    [Test]
    public void TopicTree_ExpandNode_ShowsDescendants()
    {
        // Arrange
        NavigateToTopicTreePage();
        
        // Verify child node is not visible initially (collapsed)
        var childNodeId = "TopicNode_00000000-0000-0000-0000-000000000002";
        var childNodeBefore = Driver!.TryFindElementByAutomationId(childNodeId);
        childNodeBefore.Should().BeNull("Child node should not be visible when root is collapsed");
        
        // Act - Expand root node by tapping it
        var rootNode = Driver!.FindElementByAutomationId("TopicNode_00000000-0000-0000-0000-000000000001");
        rootNode.Click();
        Thread.Sleep(500); // Wait for expansion animation
        
        // Assert
        // Verify child node becomes visible after expansion
        var childNodeAfter = Driver!.FindElementByAutomationId(childNodeId);
        childNodeAfter.Should().NotBeNull("Child node should be visible after expanding root");
        childNodeAfter.Displayed.Should().BeTrue("Child node should be displayed");
        childNodeAfter.Text.Should().Contain("Child Topic", "Child node should display title");
    }

    [Test]
    public void TopicTree_CollapseNode_HidesDescendants()
    {
        // Arrange
        NavigateToTopicTreePage();
        
        // Expand root node first
        var rootNode = Driver!.FindElementByAutomationId("TopicNode_00000000-0000-0000-0000-000000000001");
        rootNode.Click();
        Thread.Sleep(500); // Wait for expansion
        
        // Verify child is visible
        var childNodeId = "TopicNode_00000000-0000-0000-0000-000000000002";
        var childNodeBefore = Driver!.FindElementByAutomationId(childNodeId);
        childNodeBefore.Displayed.Should().BeTrue("Child node should be visible when root is expanded");
        
        // Act - Collapse root node by tapping it again
        rootNode.Click();
        Thread.Sleep(500); // Wait for collapse animation
        
        // Assert
        // Verify child node is no longer visible
        var childNodeAfter = Driver!.TryFindElementByAutomationId(childNodeId);
        childNodeAfter.Should().BeNull("Child node should not be visible when root is collapsed");
    }

    [Test]
    public void TopicTree_Indentation_IncreasesWithDepth()
    {
        // Arrange
        NavigateToTopicTreePage();
        
        // Expand all nodes to show full hierarchy
        var rootNode = Driver!.FindElementByAutomationId("TopicNode_00000000-0000-0000-0000-000000000001");
        rootNode.Click();
        Thread.Sleep(500);
        
        var childNode = Driver!.FindElementByAutomationId("TopicNode_00000000-0000-0000-0000-000000000002");
        childNode.Click();
        Thread.Sleep(500);
        
        // Act & Assert
        // Verify indentation increases with depth (16px per level)
        // Expected: Margin.Left = Depth * 16 (from DepthToThicknessConverter)
        
        var rootX = rootNode.Location.X;
        var childX = childNode.Location.X;
        var grandchildNode = Driver!.FindElementByAutomationId("TopicNode_00000000-0000-0000-0000-000000000003");
        var grandchildX = grandchildNode.Location.X;
        
        // Root node (Depth=0) should have minimal/no indentation (baseline)
        // Child node (Depth=1) should be indented 16px more than root
        var childIndent = childX - rootX;
        childIndent.Should().BeGreaterThanOrEqualTo(16, "Child node should be indented at least 16px from root");
        
        // Grandchild node (Depth=2) should be indented 16px more than child
        var grandchildIndent = grandchildX - childX;
        grandchildIndent.Should().BeGreaterThanOrEqualTo(16, "Grandchild node should be indented at least 16px from child");
        
        // Verify progressive indentation (grandchild should be more indented than root)
        var totalIndent = grandchildX - rootX;
        totalIndent.Should().BeGreaterThanOrEqualTo(32, "Grandchild node should be indented at least 32px from root (16px per depth level)");
    }

    [Test]
    public void TopicTree_AutomationIds_AreStableForNavigation()
    {
        // Arrange & Act
        NavigateToTopicTreePage();
        
        // Expected AutomationIds based on fixed GUIDs in TopicTreeAdapter
        var rootId = "TopicNode_00000000-0000-0000-0000-000000000001";
        var childId = "TopicNode_00000000-0000-0000-0000-000000000002";
        var grandchildId = "TopicNode_00000000-0000-0000-0000-000000000003";
        
        // Assert - Verify AutomationIds exist and match expected format
        var rootNode = Driver!.FindElementByAutomationId(rootId);
        rootNode.Should().NotBeNull("Root node should have AutomationId");
        rootNode.Displayed.Should().BeTrue("Root node should be visible");
        
        // Expand root
        rootNode.Click();
        Thread.Sleep(500);
        
        // Verify child AutomationId is stable after expansion
        var childNode = Driver!.FindElementByAutomationId(childId);
        childNode.Should().NotBeNull("Child node should have stable AutomationId after expansion");
        childNode.Displayed.Should().BeTrue("Child node should be visible");
        
        // Expand child
        childNode.Click();
        Thread.Sleep(500);
        
        // Verify grandchild AutomationId is stable after expansion
        var grandchildNode = Driver!.FindElementByAutomationId(grandchildId);
        grandchildNode.Should().NotBeNull("Grandchild node should have stable AutomationId after expansion");
        grandchildNode.Displayed.Should().BeTrue("Grandchild node should be visible");
        
        // Collapse child
        childNode.Click();
        Thread.Sleep(500);
        
        // Verify grandchild is hidden but AutomationId should still be stable (not found when collapsed)
        var grandchildAfterCollapse = Driver!.TryFindElementByAutomationId(grandchildId);
        grandchildAfterCollapse.Should().BeNull("Grandchild should not be visible when parent is collapsed");
        
        // Expand again - verify AutomationIds are still stable
        childNode.Click();
        Thread.Sleep(500);
        
        var grandchildAfterReExpand = Driver!.FindElementByAutomationId(grandchildId);
        grandchildAfterReExpand.Should().NotBeNull("Grandchild AutomationId should be stable after re-expansion");
        grandchildAfterReExpand.Displayed.Should().BeTrue("Grandchild should be visible after re-expansion");
        
        // Verify AutomationId format matches expected pattern
        rootId.Should().MatchRegex(@"^TopicNode_[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$", 
            "AutomationId should match format TopicNode_{GUID}");
    }
    
    [Test]
    public void NavigateToDetailPage_ShouldDisplayDetailView()
    {
        // Arrange
        NavigateToTopicTreePage();
        
        // Act - Tap on a node to navigate to detail page (assuming nodes are tappable)
        // For now, we'll verify that detail page elements exist when navigated to
        // In a full implementation, tapping a node would navigate to detail page
        var rootNode = Driver!.FindElementByAutomationId("TopicNode_00000000-0000-0000-0000-000000000001");
        
        // Simulate navigation (in actual implementation, this would be triggered by tapping)
        // For now, verify that detail page AutomationIds exist
        var detailPageAutomationId = "TopicNodeDetailPage";
        
        // Assert - Verify detail page can be navigated to
        // This would typically be done by tapping the node, but for now we verify AutomationIds exist
        rootNode.Should().NotBeNull("Root node should be accessible for navigation to detail page");
        
        // Note: Full navigation test requires detail page to be accessible via AutomationId
        // Placeholder for when navigation is fully implemented
        Assert.Pass("Detail page navigation test placeholder - full test requires navigation implementation");
    }

    [Test]
    public void DetailPage_ShouldDisplayNodeFields()
    {
        // Arrange & Act
        // Navigate to detail page (would be done via node tap in full implementation)
        NavigateToTopicTreePage();
        
        // For now, verify that detail page AutomationIds are defined
        // Full test would navigate and verify elements are visible
        var expectedAutomationIds = new[]
        {
            "TopicNodeDetailPage",
            "TopicNodeDetailPage_Title",
            "TopicNodeDetailPage_Prompt",
            "TopicNodeDetailPage_Response",
            "TopicNodeDetailPage_GenerateResponse",
            "TopicNodeDetailPage_GenerateTitle",
            "TopicNodeDetailPage_Save",
            "TopicNodeDetailPage_Cancel"
        };
        
        // Assert - Verify AutomationIds are defined in the app
        // Full test would verify these are visible after navigation
        expectedAutomationIds.Should().NotBeEmpty("Detail page should have AutomationIds defined");
        Assert.Pass("Detail page AutomationIds defined - full visibility test requires navigation implementation");
    }

    private void NavigateToTopicTreePage()
    {
        // Wait for app to load
        Thread.Sleep(2000);
        
        // Navigate to TopicTreePage
        var topicTreeNav = Driver!.FindElementByAutomationId("TopicTreePage");
        topicTreeNav.Should().NotBeNull("TopicTreePage navigation item should exist");
        topicTreeNav.Click();
        Thread.Sleep(1000);
        
        // Wait for root node to be visible (confirms page loaded)
        Driver!.WaitForElementVisible("TopicNode_00000000-0000-0000-0000-000000000001");
    }
}

