namespace Eai.Shared.Models;

public class ProcessingSummary
{
    public string GroupName { get; set; } = string.Empty;
    public int SuccessCount { get; set; }
    public int WaitingCount { get; set; }
    public int FailCount { get; set; }
    public int TotalCount => SuccessCount + WaitingCount + FailCount;
}
