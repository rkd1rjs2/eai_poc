namespace Eai.Shared.Models;

public class EaiMessage
{
    public string TraceId { get; set; } = Guid.NewGuid().ToString();
    public string IdempotencyKey { get; set; } = string.Empty;
    public string SourceSystem { get; set; } = string.Empty;
    public string TargetSystem { get; set; } = string.Empty;
    public string DataType { get; set; } = string.Empty;
    public string Payload { get; set; } = string.Empty; // JSON Serialized Data
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public int Version { get; set; } = 1;
}
