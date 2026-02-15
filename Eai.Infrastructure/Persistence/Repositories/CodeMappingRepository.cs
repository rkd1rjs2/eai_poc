using Dapper;
using System.Data;

namespace Eai.Infrastructure.Persistence.Repositories;

/// <summary>
/// 시스템 간 코드 변환(Cross-Reference) 데이터를 조회하는 저장소입니다.
/// </summary>
public class CodeMappingRepository
{
    private readonly IDbConnection _dbConnection;

    public CodeMappingRepository(IDbConnection dbConnection)
    {
        _dbConnection = dbConnection;
    }

    /// <summary>
    /// 소스 시스템의 코드를 타겟 시스템의 코드로 변환하기 위한 값을 조회합니다.
    /// </summary>
    public async Task<string?> GetTargetCodeAsync(string sourceSystem, string sourceCode, string targetSystem)
    {
        const string sql = @"
            SELECT TargetCode 
            FROM EAI_CODE_MAPPING 
            WHERE SourceSystem = @SourceSystem 
              AND SourceCode = @SourceCode 
              AND TargetSystem = @TargetSystem";

        return await _dbConnection.QueryFirstOrDefaultAsync<string>(sql, new
        {
            SourceSystem = sourceSystem,
            SourceCode = sourceCode,
            TargetSystem = targetSystem
        });
    }
}
