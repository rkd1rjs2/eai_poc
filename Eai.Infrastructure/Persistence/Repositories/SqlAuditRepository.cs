using Dapper;
using Eai.Shared.Interfaces;
using Eai.Shared.Models;
using System.Data;

namespace Eai.Infrastructure.Persistence.Repositories;

public class SqlAuditRepository : IAuditRepository
{
    private readonly IDbConnection _dbConnection;

    public SqlAuditRepository(IDbConnection dbConnection)
    {
        _dbConnection = dbConnection;
    }

    public async Task LogAsync(EaiMessage message, string status, string? errorMessage = null)
    {
        const string sql = @"
            INSERT INTO EAI_AUDIT_LOG (TraceId, IdempotencyKey, SourceSystem, TargetSystem, DataType, Status, ErrorMessage, CreatedAt)
            VALUES (@TraceId, @IdempotencyKey, @SourceSystem, @TargetSystem, @DataType, @Status, @ErrorMessage, @CreatedAt)";

        await _dbConnection.ExecuteAsync(sql, new
        {
            message.TraceId,
            message.IdempotencyKey,
            message.SourceSystem,
            message.TargetSystem,
            message.DataType,
            Status = status,
            ErrorMessage = errorMessage,
            CreatedAt = DateTime.UtcNow
        });
    }

    public async Task UpdateStatusAsync(string traceId, string status, string? errorMessage = null)
    {
        const string sql = @"
            UPDATE EAI_AUDIT_LOG 
            SET Status = @Status, ErrorMessage = @ErrorMessage, UpdatedAt = @UpdatedAt
            WHERE TraceId = @TraceId";

        await _dbConnection.ExecuteAsync(sql, new
        {
            TraceId = traceId,
            Status = status,
            ErrorMessage = errorMessage,
            UpdatedAt = DateTime.UtcNow
        });
    }

    public async Task<AuditLogModel?> GetAuditLogAsync(string traceId)
    {
        const string sql = "SELECT * FROM EAI_AUDIT_LOG WHERE TraceId = @traceId";
        return await _dbConnection.QuerySingleOrDefaultAsync<AuditLogModel>(sql, new { traceId });
    }

    public async Task<IEnumerable<ProcessingSummary>> GetSystemSummaryAsync(DateTime date)
    {
        const string sql = @"
            SELECT 
                SourceSystem as GroupName,
                COUNT(*) FILTER (WHERE Status = 'SUCCESS') as SuccessCount,
                COUNT(*) FILTER (WHERE Status IN ('READY', 'PROCESSING')) as WaitingCount,
                COUNT(*) FILTER (WHERE Status = 'FAIL') as FailCount
            FROM EAI_AUDIT_LOG
            WHERE CreatedAt >= @Date AND CreatedAt < @NextDate
            GROUP BY SourceSystem";

        return await _dbConnection.QueryAsync<ProcessingSummary>(sql, new 
        { 
            Date = date.Date, 
            NextDate = date.Date.AddDays(1) 
        });
    }

    public async Task<IEnumerable<ProcessingSummary>> GetBusinessSummaryAsync(DateTime date)
    {
        const string sql = @"
            SELECT 
                DataType as GroupName,
                COUNT(*) FILTER (WHERE Status = 'SUCCESS') as SuccessCount,
                COUNT(*) FILTER (WHERE Status IN ('READY', 'PROCESSING')) as WaitingCount,
                COUNT(*) FILTER (WHERE Status = 'FAIL') as FailCount
            FROM EAI_AUDIT_LOG
            WHERE CreatedAt >= @Date AND CreatedAt < @NextDate
            GROUP BY DataType";

        return await _dbConnection.QueryAsync<ProcessingSummary>(sql, new 
        { 
            Date = date.Date, 
            NextDate = date.Date.AddDays(1) 
        });
    }
}
