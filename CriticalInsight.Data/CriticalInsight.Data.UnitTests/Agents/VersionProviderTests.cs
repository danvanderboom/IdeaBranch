using System;
using System.Linq;
using System.Threading.Tasks;
using CriticalInsight.Data.Agents;
using NUnit.Framework;

namespace CriticalInsight.Data.UnitTests.Agents;

[TestFixture]
public class VersionProviderTests
{
    [Test]
    public void TryCheckAndBump_ConflictsWhenStale()
    {
        var vp = new InMemoryVersionProvider();
        var scope = "scope-1";

        // Initial bump (no expected version) succeeds to 1
        Assert.That(vp.TryCheckAndBump(scope, null, out var v1), Is.True);
        Assert.That(v1, Is.EqualTo("1"));

        // Using stale token "0" should conflict and return current (1)
        Assert.That(vp.TryCheckAndBump(scope, "0", out var cur), Is.False);
        Assert.That(cur, Is.EqualTo("1"));
    }

    [Test]
    public async Task ConcurrentBumps_MonotonicAndAccurate()
    {
        var vp = new InMemoryVersionProvider();
        var scope = "scope-concurrent";
        int tasks = 16;

        var work = Enumerable.Range(0, tasks)
            .Select(_ => Task.Run(() =>
            {
                Assert.That(vp.TryCheckAndBump(scope, null, out string _), Is.True);
            }))
            .ToArray();

        await Task.WhenAll(work);

        // Version should equal number of successful bumps
        Assert.That(vp.GetVersion(scope), Is.EqualTo(tasks.ToString()));
    }
}


