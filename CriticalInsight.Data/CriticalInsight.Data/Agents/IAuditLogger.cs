using System;

namespace CriticalInsight.Data.Agents;

public sealed class AuditLogEntry
{
    public DateTime TimestampUtc { get; set; } = DateTime.UtcNow;
    public string AgentId { get; set; } = string.Empty;
    public string Operation { get; set; } = string.Empty;
    public string Target { get; set; } = string.Empty;
    public bool Success { get; set; }
    public string? ErrorCode { get; set; }
    public string? ErrorMessage { get; set; }
}

public interface IAuditLogger
{
    void Log(AuditLogEntry entry);
}

public sealed class NoopAuditLogger : IAuditLogger
{
    public void Log(AuditLogEntry entry) { }
}


