using System;
using System.Threading.Tasks;
using CriticalInsight.Data.Agents;
using NUnit.Framework;

namespace CriticalInsight.Data.UnitTests.Agents;

[TestFixture]
public class RateLimiterTests
{
    [Test]
    public void PerAgentIsolation_Works()
    {
        var limiter = new TokenBucketRateLimiter(capacity: 1, refillPeriod: TimeSpan.FromSeconds(60));

        // First agent consumes its single token
        Assert.That(limiter.TryConsume("agent-a", out _), Is.True);
        // Second agent still has its own bucket
        Assert.That(limiter.TryConsume("agent-b", out _), Is.True);
        // Agent A now rate-limited
        Assert.That(limiter.TryConsume("agent-a", out var retryA), Is.False);
        Assert.That(retryA, Is.Not.Null);
    }

    [Test]
    public async Task Refill_AllowsSubsequentRequests()
    {
        var limiter = new TokenBucketRateLimiter(capacity: 1, refillPeriod: TimeSpan.FromMilliseconds(200));

        Assert.That(limiter.TryConsume("agent", out _), Is.True);
        Assert.That(limiter.TryConsume("agent", out _), Is.False);

        await Task.Delay(250);

        Assert.That(limiter.TryConsume("agent", out _), Is.True);
    }
}


