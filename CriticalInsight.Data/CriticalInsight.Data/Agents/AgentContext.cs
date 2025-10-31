using System;
using System.Collections.Generic;

namespace CriticalInsight.Data.Agents;

public enum AgentRole
{
    Reader,
    Editor
}

public sealed class AgentContext
{
    public string AgentId { get; }

    public bool ReadOnly { get; }

    public IReadOnlyCollection<AgentRole> Roles { get; }

    public AgentContext(string agentId, bool readOnly, IEnumerable<AgentRole> roles)
    {
        AgentId = agentId;
        ReadOnly = readOnly;
        Roles = new List<AgentRole>(roles);
    }

    public bool HasRole(AgentRole role) => Roles.Contains(role);
}


