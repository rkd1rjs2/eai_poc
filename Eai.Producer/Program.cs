using Eai.Infrastructure.Persistence.Repositories;
using Eai.Infrastructure.Redis;
using Eai.Shared.Constants;
using Eai.Shared.Models;
using Eai.Shared.Utils;
using Microsoft.Extensions.Configuration;
using Npgsql;
using Oracle.ManagedDataAccess.Client;
using StackExchange.Redis;
using System.Data;
using Dapper;

// --- EAI Producer: 다중 DB 지원 실시간 데이터 캡처 모듈 ---

var configuration = new ConfigurationBuilder()
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddEnvironmentVariables() // 이 줄이 추가되어야 도커 환경 변수를 읽습니다.
    .Build();

string eaiCoreConn = configuration.GetConnectionString("EaiCoreDb")!;
string hrConn = configuration.GetConnectionString("HrDb")!;
string acConn = configuration.GetConnectionString("AcDb")!;
string arConn = configuration.GetConnectionString("ArDb")!;

string redisConnString = configuration.GetValue<string>("Redis:ConnectionString") ?? "localhost:6380";
string sourceSystem = configuration.GetValue<string>("EaiSettings:SourceSystem") ?? "UNKNOWN";
string targetSystem = configuration.GetValue<string>("EaiSettings:TargetSystem") ?? "UNKNOWN";

Console.WriteLine("[Producer] Starting Multi-DB Monitor...");

// 인프라 초기화 (공통 로그는 eai_core_db 사용)
using IDbConnection auditDb = new NpgsqlConnection(eaiCoreConn);
var redis = await ConnectionMultiplexer.ConnectAsync(redisConnString + ",abortConnect=false");
var auditRepo = new SqlAuditRepository(auditDb);
var publisher = new RedisStreamPublisher(redis);

// 각 도메인 DB 리스너 실행
var tasks = new List<Task>
{
    RunDomainListener(hrConn, "hr_mst_changed", "HR_INFO", "HR_STREAM"),
    RunDomainListener(acConn, "ac_mst_changed", "AC_INFO", "AC_STREAM"),
    RunDomainListener(arConn, "ar_mst_changed", "AR_INFO", "AR_STREAM")
};

await Task.WhenAll(tasks);

async Task RunDomainListener(string connStr, string channel, string dataType, string stream)
{
    using var conn = new NpgsqlConnection(connStr);
    await conn.OpenAsync();
    
    using (var cmd = new NpgsqlCommand($"LISTEN {channel}", conn))
    {
        await cmd.ExecuteNonQueryAsync();
    }

    Console.WriteLine($"[Listener] Started on {conn.Database}, Channel: {channel}");

    conn.Notification += async (o, e) =>
    {
        try 
        {
            var payload = System.Text.Json.JsonDocument.Parse(e.Payload);
            string id = payload.RootElement.GetProperty("id").GetString()!;
            
            using var sourceDb = new NpgsqlConnection(connStr);
            object? data = null;

            if (dataType == "HR_INFO")
                data = await sourceDb.QuerySingleOrDefaultAsync<EmployeeDto>("SELECT * FROM SOURCE_HR_MST WHERE EMP_NO = @id", new { id });
            else if (dataType == "AC_INFO")
                data = await sourceDb.QuerySingleOrDefaultAsync("SELECT * FROM SOURCE_AC_MST WHERE AC_ID = @id", new { id });
            else if (dataType == "AR_INFO")
                data = await sourceDb.QuerySingleOrDefaultAsync("SELECT * FROM SOURCE_AR_MST WHERE AR_ID = @id", new { id });

            if (data != null)
            {
                await ProcessAndSend(data, stream, dataType, sourceSystem, targetSystem, auditRepo, publisher);
            }
        }
        catch (Exception ex) { Console.WriteLine($"[Error] {dataType} Listener Error: {ex.Message}"); }
    };

    while (true) await conn.WaitAsync();
}

/// <summary>
/// Oracle용 DCN/CQN (Database Change Notification) 핸들러
/// </summary>
async Task RunOracleCqnListener(string connStr, string stream, string source, string target, SqlAuditRepository audit, RedisStreamPublisher pub)
{
    /* 
       [Oracle DCN/CQN 상세 설명]
       1. 원리: DB 서버에 감시할 쿼리를 등록하면, 해당 데이터 변경 시 DB가 이 앱으로 TCP 신호를 줍니다.
       2. 트랜잭션: 알림은 트랜잭션이 'COMMIT' 될 때 발생하며, 묶음 처리가 가능합니다.
       3. 위치 투명성: 이 프로그램이 DB 외부(다른 서버)에 있어도 DB가 IP/Port를 통해 역으로 접속합니다.
    */

    using var conn = new OracleConnection(connStr);
    await conn.OpenAsync();

    // 1. 알림 옵션 설정
    // QOS_RELIABLE: DB 재시작 후에도 알림 보장
    // QOS_ROWIDS: 변경된 행의 ROWID 정보를 포함
    OracleCommand cmd = conn.CreateCommand();
    cmd.CommandText = "SELECT EMP_NO, NAME, DEPT_CODE, SALARY FROM SOURCE_HR_DATA";
    
    OracleDependency dep = new OracleDependency(cmd);
    dep.QueryBasedNotification = true; // CQN 모드 활성화
    
    // 2. 이벤트 핸들러 등록
    dep.OnChange += async (o, e) =>
    {
        // e.Details를 통해 어떤 데이터가(RowId 등) 변경되었는지 확인 가능
        // 트랜잭션 단위로 묶인 알림이 이 핸들러 한 번에 전달됩니다.
        Console.WriteLine($"[Oracle CQN] Change Detected. Info: {e.Info}, Source: {e.Source}");

        if (e.Info == OracleNotificationInfo.Insert || e.Info == OracleNotificationInfo.Update)
        {
            // 실제 운영에서는 e.Details의 Rowid를 사용하여 변경된 데이터만 다시 쿼리합니다.
            using var fetchConn = new OracleConnection(connStr);
            var empList = await fetchConn.QueryAsync<EmployeeDto>(
                "SELECT EMP_NO, NAME, DEPT_CODE, SALARY FROM SOURCE_HR_DATA"); // 예시를 위해 전체 조회

            foreach (var emp in empList)
            {
                await ProcessAndSend(emp, stream, "HR_INFO", source, target, audit, pub);
            }
        }
    };

    // 3. 쿼리 등록 실행 (최초 1회 실행하여 DB에 등록)
    await cmd.ExecuteReaderAsync();

    Console.WriteLine("[Oracle] CQN Registered. Waiting for notifications...");
    
    // 알림은 백그라운드 스레드에서 수신되므로 메인 스레드는 대기합니다.
    while (true) await Task.Delay(10000);
}

async Task ProcessAndSend(object data, string stream, string dataType, string source, string target, SqlAuditRepository audit, RedisStreamPublisher pub)
{
    // 데이터 타입에 따라 ID 추출 (간단하게 dynamic 또는 패턴 매칭 사용)
    string idValue = data is EmployeeDto e ? e.EMP_NO : 
                     data is IDictionary<string, object> dict ? (dict.ContainsKey("ac_id") ? dict["ac_id"].ToString()! : dict.ContainsKey("ar_id") ? dict["ar_id"].ToString()! : "N/A") : 
                     "UNKNOWN";
                     
    string idempotencyKey = IdempotencyKeyHelper.Create(idValue, DateTime.UtcNow.ToString("yyyyMMddHHmm"));
    var eaiMessage = new EaiMessage
    {
        TraceId = TraceIdGenerator.Generate(),
        IdempotencyKey = idempotencyKey,
        SourceSystem = source,
        TargetSystem = target,
        DataType = dataType,
        Payload = System.Text.Json.JsonSerializer.Serialize(data),
        Timestamp = DateTime.UtcNow
    };

    await audit.LogAsync(eaiMessage, MessageStatus.Ready);
    await pub.PublishAsync(stream, eaiMessage);
    Console.WriteLine($"[Sent] Type: {dataType}, ID: {idValue}, TraceId: {eaiMessage.TraceId} -> Stream: {stream}");
}

public class EmployeeDto
{
    public string EMP_NO { get; set; } = string.Empty;
    public string NAME { get; set; } = string.Empty;
    public string DEPT_CODE { get; set; } = string.Empty;
    public decimal SALARY { get; set; }
}