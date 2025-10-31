using System;
using System.Collections.Concurrent;

namespace CriticalInsight.Data.Agents;

public interface IIdempotencyStore
{
    bool TryGet(string agentId, string key, out object? result);
    void Put(string agentId, string key, object result, TimeSpan ttl);
}

public sealed class InMemoryIdempotencyStore : IIdempotencyStore
{
    private sealed class Entry
    {
        public object Result = new();
        public DateTime ExpiresUtc;
    }

    private readonly ConcurrentDictionary<string, Entry> _entries = new();

    private static string MakeKey(string agentId, string key) => agentId + "::" + key;

    public bool TryGet(string agentId, string key, out object? result)
    {
        var k = MakeKey(agentId, key);
        if (_entries.TryGetValue(k, out var entry))
        {
            if (entry.ExpiresUtc > DateTime.UtcNow)
            {
                result = entry.Result;
                return true;
            }
            _entries.TryRemove(k, out _);
        }
        result = null;
        return false;
    }

    public void Put(string agentId, string key, object result, TimeSpan ttl)
    {
        var k = MakeKey(agentId, key);
        _entries[k] = new Entry { Result = result, ExpiresUtc = DateTime.UtcNow.Add(ttl) };
    }
}


