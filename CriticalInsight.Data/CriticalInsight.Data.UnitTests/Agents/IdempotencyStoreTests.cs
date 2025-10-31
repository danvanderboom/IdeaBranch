using System;
using System.Threading.Tasks;
using CriticalInsight.Data.Agents;
using NUnit.Framework;

namespace CriticalInsight.Data.UnitTests.Agents;

[TestFixture]
public class IdempotencyStoreTests
{
    [Test]
    public void PerAgentKeys_AreIsolated()
    {
        var store = new InMemoryIdempotencyStore();
        var result = new object();

        store.Put("agent-1", "key", result, TimeSpan.FromSeconds(5));

        Assert.That(store.TryGet("agent-1", "key", out var found1), Is.True);
        Assert.That(found1, Is.SameAs(result));

        Assert.That(store.TryGet("agent-2", "key", out var found2), Is.False);
        Assert.That(found2, Is.Null);
    }

    [Test]
    public async Task TtlExpiry_RemovesEntries()
    {
        var store = new InMemoryIdempotencyStore();
        var result = new object();

        store.Put("agent", "key", result, TimeSpan.FromMilliseconds(50));
        Assert.That(store.TryGet("agent", "key", out var found), Is.True);
        Assert.That(found, Is.SameAs(result));

        await Task.Delay(80);

        Assert.That(store.TryGet("agent", "key", out var after), Is.False);
        Assert.That(after, Is.Null);
    }
}


