using Eai.Shared.Models;

namespace Eai.Shared.Interfaces;

public interface IAuditRepository
{
    Task LogAsync(EaiMessage message, string status, string? errorMessage = null);
    Task UpdateStatusAsync(string traceId, string status, string? errorMessage = null);
    Task<AuditLogModel?> GetAuditLogAsync(string traceId);
    Task<IEnumerable<ProcessingSummary>> GetSystemSummaryAsync(DateTime date);
    Task<IEnumerable<ProcessingSummary>> GetBusinessSummaryAsync(DateTime date);
}

public class AuditLogModel
{
    public string TraceId { get; set; } = string.Empty;
    public string IdempotencyKey { get; set; } = string.Empty;
    public string SourceSystem { get; set; } = string.Empty;
    public string TargetSystem { get; set; } = string.Empty;
    public string DataType { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? ErrorMessage { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
