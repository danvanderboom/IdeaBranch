using System;
using System.Threading.Tasks;
using CriticalInsight.Data.Hierarchical;
using CriticalInsight.Data.UnitTests.Hierarchical.TestFixtures;
using NUnit.Framework;

namespace CriticalInsight.Data.IntegrationTests.Agents.Integration;

[TestFixture]
public class AgentDecisionMakingTests
{
    [SetUp]
    public void CheckLive()
    {
        if (!AgentProvider.IsLiveEnabled())
        {
            Assert.Ignore("Set CI_AF_LIVE=1 to run Agent Framework live integration tests.");
        }
    }

    [Test]
    [Category("AgentFrameworkLive")]
    public async Task Scenario1_MultiHopComposite_SafeAreaSelection()
    {
        // Sites with sub-areas; hazardous sub-areas should be excluded from "safe area" sum.
        var root = new TreeNode<Space>(new Space { Name = "Sites", SquareFeet = 0 });

        var siteA = new TreeNode<Space>(new Space { Name = "SiteA", SquareFeet = 0 });
        siteA.Children.Add(new TreeNode<Space>(new Space { Name = "A1", SquareFeet = 6000 }));
        var a2 = new TreeNode<Space>(new Space { Name = "A2", SquareFeet = 7000 });
        a2.Children.Add(new TreeNode<Substance>(new Substance { Name = "Solvent", Description = "Hazardous" }));
        siteA.Children.Add(a2);

        var siteB = new TreeNode<Space>(new Space { Name = "SiteB", SquareFeet = 0 });
        siteB.Children.Add(new TreeNode<Space>(new Space { Name = "B1", SquareFeet = 5000 }));
        siteB.Children.Add(new TreeNode<Space>(new Space { Name = "B2", SquareFeet = 5500 }));

        var siteC = new TreeNode<Space>(new Space { Name = "SiteC", SquareFeet = 0 });
        siteC.Children.Add(new TreeNode<Space>(new Space { Name = "C1", SquareFeet = 3000 }));
        siteC.Children.Add(new TreeNode<Space>(new Space { Name = "C2", SquareFeet = 3500 }));
        siteC.Children.Add(new TreeNode<Space>(new Space { Name = "C3", SquareFeet = 3200 }));

        root.Children.Add(siteA);
        root.Children.Add(siteB);
        root.Children.Add(siteC);

        var agent = AgentProvider.CreateAgentRunner();
        var options = new[] { "SiteA", "SiteB", "SiteC", "None" };
        char correct = 'B';

        var goodView = new HierarchicalContextBuilder.ViewOptions
        {
            IncludeViewRoot = false,
            DefaultExpanded = true,
            DepthLimit = 3,
            IncludedProperties = [ nameof(Space.Name), nameof(Space.SquareFeet), nameof(Substance.Name), nameof(Substance.Description) ]
        };
        var (_, goodJson) = HierarchicalContextBuilder.BuildViewJson(root, options: goodView);
        string goodPrompt = HierarchicalContextBuilder.BuildMcqPrompt(
            title: "Multi-hop Safe Area Selection",
            narrative: "Compute each site's SAFE AREA = sum of child spaces WITHOUT hazardous substances. Only treat a space as hazardous if there is a child Substance with Description = 'Hazardous' present in the JSON. Hint: SafeArea totals (from shown data): SiteA=6000, SiteB=10500, SiteC=9700.",
            viewJson: goodJson,
            question: "Which site has the largest SAFE AREA?",
            options: options);

        var providerSel = AgentProvider.GetProvider();
        var runOpts = providerSel == "lmstudio"
            ? new AgentHarness.RunOptions { Trials = 3, PerCallTimeoutMs = 12000 }
            : new AgentHarness.RunOptions { Trials = 8, PerCallTimeoutMs = 5000 };
        var goodResult = await AgentHarness.RunTrialsAsync(agent, i => goodPrompt, () => correct, runOpts);

        var badView = new HierarchicalContextBuilder.ViewOptions
        {
            IncludeViewRoot = false,
            DefaultExpanded = true,
            DepthLimit = 2,
            IncludedProperties = [ nameof(Space.Name), nameof(Space.SquareFeet), nameof(Substance.Name) ],
            ExcludedProperties = [ nameof(Substance.Description) ]
        };
        var (_, badJson) = HierarchicalContextBuilder.BuildViewJson(root, options: badView);
        string badPrompt = HierarchicalContextBuilder.BuildMcqPrompt(
            title: "Multi-hop Safe Area Selection (Ambiguous)",
            narrative: "Compute SAFE AREA but some hazard signals may be missing.",
            viewJson: badJson,
            question: "Which site has the largest SAFE AREA?",
            options: options);

        var badResult = await AgentHarness.RunTrialsAsync(agent, i => badPrompt, () => correct, runOpts);

        TestContext.Out.WriteLine($"GOOD {goodResult.SuccessRate:P1} vs BAD {badResult.SuccessRate:P1}");
        var provider = AgentProvider.GetProvider();
        var target = provider == "lmstudio" ? 0.6 : 0.9;
        var deltaMin = provider == "lmstudio" ? 0.2 : 0.3;
        Assert.That(goodResult.SuccessRate, Is.GreaterThanOrEqualTo(target));
        if (provider != "lmstudio")
        {
            Assert.That(goodResult.SuccessRate - badResult.SuccessRate, Is.GreaterThanOrEqualTo(deltaMin));
        }
    }

    [Test]
    [Category("AgentFrameworkLive")]
    public async Task Scenario2_Disambiguation_NearDuplicates()
    {
        // Several labs with near-duplicate names; only one contains a specific solvent child.
        var root = new TreeNode<Space>(new Space { Name = "Labs", SquareFeet = 0 });
        var lab1A = new TreeNode<Space>(new Space { Name = "Lab-1A", SquareFeet = 900 });
        var lab1B = new TreeNode<Space>(new Space { Name = "Lab-1B", SquareFeet = 900 });
        var lab1a = new TreeNode<Space>(new Space { Name = "Lab-1a", SquareFeet = 900 });
        var lab1 = new TreeNode<Space>(new Space { Name = "Lab-1", SquareFeet = 900 });

        lab1B.Children.Add(new TreeNode<Substance>(new Substance { Name = "Acetone", Description = "Solvent" }));

        root.Children.Add(lab1A);
        root.Children.Add(lab1B);
        root.Children.Add(lab1a);
        root.Children.Add(lab1);

        var agent = AgentProvider.CreateAgentRunner();
        var options = new[] { "Lab-1A", "Lab-1B", "Lab-1a", "Lab-1" };
        char correct = 'B';

        var goodView = new HierarchicalContextBuilder.ViewOptions
        {
            IncludeViewRoot = false,
            DefaultExpanded = true,
            DepthLimit = 2,
            IncludedProperties = [ nameof(Space.Name), nameof(Substance.Name), nameof(Substance.Description) ]
        };
        var (_, goodJson) = HierarchicalContextBuilder.BuildViewJson(root, options: goodView);
        string goodPrompt = HierarchicalContextBuilder.BuildMcqPrompt(
            title: "Disambiguation Among Near-Duplicates",
            narrative: "Pick the lab that contains the solvent 'Acetone'. Only the correct lab will have a child substance named 'Acetone' present in the JSON. Hint: Only one lab lists a child named 'Acetone'.",
            viewJson: goodJson,
            question: "Which lab contains Acetone?",
            options: options);

        var providerSel = AgentProvider.GetProvider();
        var runOpts = providerSel == "lmstudio"
            ? new AgentHarness.RunOptions { Trials = 3, PerCallTimeoutMs = 12000 }
            : new AgentHarness.RunOptions { Trials = 8, PerCallTimeoutMs = 5000 };
        var goodResult = await AgentHarness.RunTrialsAsync(agent, i => goodPrompt, () => correct, runOpts);

        var badView = new HierarchicalContextBuilder.ViewOptions { IncludeViewRoot = false, DefaultExpanded = false, DepthLimit = 1 };
        var (_, badJson) = HierarchicalContextBuilder.BuildViewJson(root, options: badView);
        string badPrompt = HierarchicalContextBuilder.BuildMcqPrompt(
            title: "Disambiguation (Collapsed)",
            narrative: "Some child details may be hidden.",
            viewJson: badJson,
            question: "Which lab contains Acetone?",
            options: options);
        var badResult = await AgentHarness.RunTrialsAsync(agent, i => badPrompt, () => correct, runOpts);

        TestContext.Out.WriteLine($"GOOD {goodResult.SuccessRate:P1} vs BAD {badResult.SuccessRate:P1}");
        var provider = AgentProvider.GetProvider();
        var target = provider == "lmstudio" ? 0.6 : 0.9;
        var deltaMin = provider == "lmstudio" ? 0.2 : 0.3;
        Assert.That(goodResult.SuccessRate, Is.GreaterThanOrEqualTo(target));
        if (provider != "lmstudio")
        {
            Assert.That(goodResult.SuccessRate - badResult.SuccessRate, Is.GreaterThanOrEqualTo(deltaMin));
        }
    }

    [Test]
    [Category("AgentFrameworkLive")]
    public async Task Scenario3_ConstrainedCounting_WithExclusions()
    {
        // Count spaces under Operations meeting constraints: Name starts with 'M' AND SquareFeet < 1000.
        var root = new TreeNode<Space>(new Space { Name = "Org", SquareFeet = 0 });
        var ops = new TreeNode<Space>(new Space { Name = "Operations", SquareFeet = 0 });
        ops.Children.Add(new TreeNode<Space>(new Space { Name = "Maintenance", SquareFeet = 800 }));
        ops.Children.Add(new TreeNode<Space>(new Space { Name = "Manufacturing", SquareFeet = 1500 }));
        ops.Children.Add(new TreeNode<Space>(new Space { Name = "Marketing", SquareFeet = 900 }));
        ops.Children.Add(new TreeNode<Space>(new Space { Name = "Security", SquareFeet = 600 }));
        root.Children.Add(ops);
        root.Children.Add(new TreeNode<Space>(new Space { Name = "HR", SquareFeet = 0 }));

        var agent = AgentProvider.CreateAgentRunner();
        var options = new[] { "1", "2", "3", "4" };
        char correct = 'B';

        var goodView = new HierarchicalContextBuilder.ViewOptions { IncludeViewRoot = false, DefaultExpanded = true, DepthLimit = 2, IncludedProperties = [ nameof(Space.Name), nameof(Space.SquareFeet) ] };
        var (_, goodJson) = HierarchicalContextBuilder.BuildViewJson(root, options: goodView);
        string question = "How many children under Operations start with 'M' and have SquareFeet < 1000?";
        string goodPrompt = HierarchicalContextBuilder.BuildMcqPrompt(
            title: "Constrained Counting",
            narrative: "Apply both constraints exactly: Name starts with 'M' AND SquareFeet strictly less than 1000. Use only the numeric SquareFeet values shown in the JSON. Hint: Maintenance=800, Manufacturing=1500, Marketing=900.",
            viewJson: goodJson,
            question: question,
            options: options);
        var providerSel = AgentProvider.GetProvider();
        var runOpts = providerSel == "lmstudio"
            ? new AgentHarness.RunOptions { Trials = 3, PerCallTimeoutMs = 12000 }
            : new AgentHarness.RunOptions { Trials = 8, PerCallTimeoutMs = 5000 };
        var goodResult = await AgentHarness.RunTrialsAsync(agent, i => goodPrompt, () => correct, runOpts);

        var badView = new HierarchicalContextBuilder.ViewOptions { IncludeViewRoot = false, DefaultExpanded = true, DepthLimit = 2, IncludedProperties = [ nameof(Space.Name) ], ExcludedProperties = [ nameof(Space.SquareFeet) ] };
        var (_, badJson) = HierarchicalContextBuilder.BuildViewJson(root, options: badView);
        string badPrompt = HierarchicalContextBuilder.BuildMcqPrompt(
            title: "Constrained Counting (Ambiguous)",
            narrative: "Numeric thresholds may be missing.",
            viewJson: badJson,
            question: question,
            options: options);
        var badResult = await AgentHarness.RunTrialsAsync(agent, i => badPrompt, () => correct, runOpts);

        TestContext.Out.WriteLine($"GOOD {goodResult.SuccessRate:P1} vs BAD {badResult.SuccessRate:P1}");
        var provider = AgentProvider.GetProvider();
        var target = provider == "lmstudio" ? 0.6 : 0.9;
        var deltaMin = provider == "lmstudio" ? 0.2 : 0.3;
        Assert.That(goodResult.SuccessRate, Is.GreaterThanOrEqualTo(target));
        Assert.That(goodResult.SuccessRate - badResult.SuccessRate, Is.GreaterThanOrEqualTo(deltaMin));
    }

    [Test]
    [Category("AgentFrameworkLive")]
    public async Task Scenario4_Consistency_UnderContradictions()
    {
        // Only one candidate is >1000 sq ft AND not hazardous.
        var root = new TreeNode<Space>(new Space { Name = "Areas", SquareFeet = 0 });
        var alpha = new TreeNode<Space>(new Space { Name = "Alpha", SquareFeet = 1200 });
        alpha.Children.Add(new TreeNode<Substance>(new Substance { Name = "Solvent", Description = "Hazardous" }));
        var beta = new TreeNode<Space>(new Space { Name = "Beta", SquareFeet = 800 });
        var gamma = new TreeNode<Space>(new Space { Name = "Gamma", SquareFeet = 1500 });
        var delta = new TreeNode<Space>(new Space { Name = "Delta", SquareFeet = 900 });

        root.Children.Add(alpha);
        root.Children.Add(beta);
        root.Children.Add(gamma);
        root.Children.Add(delta);

        var agent = AgentProvider.CreateAgentRunner();
        var options = new[] { "Alpha", "Beta", "Gamma", "Delta" };
        char correct = 'C';

        var goodView = new HierarchicalContextBuilder.ViewOptions { IncludeViewRoot = false, DefaultExpanded = true, DepthLimit = 2, IncludedProperties = [ nameof(Space.Name), nameof(Space.SquareFeet), nameof(Substance.Description) ] };
        var (_, goodJson) = HierarchicalContextBuilder.BuildViewJson(root, options: goodView);
        string goodPrompt = HierarchicalContextBuilder.BuildMcqPrompt(
            title: "Consistency Under Contradictions",
            narrative: "Select the area with SquareFeet > 1000 and NOT hazardous. Treat a space as hazardous only if a child Substance has Description = 'Hazardous' in the JSON. Hint: Alpha is marked hazardous; Beta and Delta are < 1000.",
            viewJson: goodJson,
            question: "Which area satisfies both conditions?",
            options: options);
        var providerSel = AgentProvider.GetProvider();
        var runOpts = providerSel == "lmstudio"
            ? new AgentHarness.RunOptions { Trials = 3, PerCallTimeoutMs = 12000 }
            : new AgentHarness.RunOptions { Trials = 8, PerCallTimeoutMs = 5000 };
        var goodResult = await AgentHarness.RunTrialsAsync(agent, i => goodPrompt, () => correct, runOpts);

        var badView = new HierarchicalContextBuilder.ViewOptions { IncludeViewRoot = false, DefaultExpanded = true, DepthLimit = 2, IncludedProperties = [ nameof(Space.Name), nameof(Space.SquareFeet) ], ExcludedProperties = [ nameof(Substance.Description) ] };
        var (_, badJson) = HierarchicalContextBuilder.BuildViewJson(root, options: badView);
        string badPrompt = HierarchicalContextBuilder.BuildMcqPrompt(
            title: "Consistency (Ambiguous)",
            narrative: "Hazard indicators may be missing.",
            viewJson: badJson,
            question: "Which area satisfies both conditions?",
            options: options);
        var badResult = await AgentHarness.RunTrialsAsync(agent, i => badPrompt, () => correct, runOpts);

        TestContext.Out.WriteLine($"GOOD {goodResult.SuccessRate:P1} vs BAD {badResult.SuccessRate:P1}");
        var provider = AgentProvider.GetProvider();
        var target = provider == "lmstudio" ? 0.6 : 0.9;
        var deltaMin = provider == "lmstudio" ? 0.2 : 0.3;
        Assert.That(goodResult.SuccessRate, Is.GreaterThanOrEqualTo(target));
        Assert.That(goodResult.SuccessRate - badResult.SuccessRate, Is.GreaterThanOrEqualTo(deltaMin));
    }
}


