using System;
using System.Collections.Concurrent;

namespace CriticalInsight.Data.Agents;

public interface IVersionProvider
{
    string GetVersion(string scopeId);
    bool TryCheckAndBump(string scopeId, string? expectedVersion, out string newVersion);
}

public sealed class InMemoryVersionProvider : IVersionProvider
{
    private readonly ConcurrentDictionary<string, long> _versions = new();

    public string GetVersion(string scopeId)
    {
        var v = _versions.GetOrAdd(scopeId, 0);
        return v.ToString();
    }

    public bool TryCheckAndBump(string scopeId, string? expectedVersion, out string newVersion)
    {
        while (true)
        {
            var current = _versions.GetOrAdd(scopeId, 0);
            if (expectedVersion != null && expectedVersion != current.ToString())
            {
                newVersion = current.ToString();
                return false;
            }
            var updated = current + 1;
            if (_versions.TryUpdate(scopeId, updated, current))
            {
                newVersion = updated.ToString();
                return true;
            }
        }
    }
}


