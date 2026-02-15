using Eai.Infrastructure.Persistence.Repositories;
using Eai.Infrastructure.Redis;
using Eai.Shared.Constants;
using Eai.Shared.Interfaces;
using Eai.Shared.Models;
using Eai.Shared.Utils;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Npgsql;
using StackExchange.Redis;
using System.Data;
using System.Text.Json;

namespace Eai.Transformer;

/// <summary>
/// Redis Stream에서 메시지를 소비하여 변환 로직을 수행하고 타겟으로 전달하는 백그라운드 워커 서비스입니다.
/// </summary>
public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IConfiguration _configuration;
    private readonly string _consumerName = Guid.NewGuid().ToString("N");

    public Worker(ILogger<Worker> logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // 1. 설정 로드
        string auditConnString = _configuration.GetConnectionString("AuditDb")!;
        string redisConnString = _configuration.GetValue<string>("Redis:ConnectionString")!;
        string streamName = _configuration.GetValue<string>("EaiSettings:StreamName")!;
        string groupName = _configuration.GetValue<string>("EaiSettings:ConsumerGroup")!;

        _logger.LogInformation("[Transformer] Starting worker for stream: {Stream}, group: {Group}", streamName, groupName);

        // 2. 인프라 서비스 초기화
        using var redis = await ConnectionMultiplexer.ConnectAsync(redisConnString);
        var consumer = new RedisStreamConsumer(redis);
        var publisher = new RedisStreamPublisher(redis); // AR 체이닝용
        using var httpClient = new HttpClient(); // AC 웹훅용
        
        // 소비자 그룹 보장
        string[] streams = { streamName, "AC_STREAM", "AR_STREAM" };
        foreach (var s in streams)
        {
            await consumer.CreateConsumerGroupAsync(s, groupName);
        }

        // 루프 시작
        while (!stoppingToken.IsCancellationRequested)
        {
            foreach (var currentStream in streams)
            {
                try
                {
                    var messages = await consumer.ReadMessagesAsync(currentStream, groupName, _consumerName);
                    foreach (var entry in messages)
                    {
                        var jsonValue = entry.Values.FirstOrDefault(v => v.Name == "data").Value;
                        string? json = (string?)jsonValue;
                        if (string.IsNullOrEmpty(json)) continue;

                        var eaiMessage = JsonSerializer.Deserialize<EaiMessage>(json);
                        if (eaiMessage == null) continue;

                        _logger.LogInformation("[{Stream}] Processing TraceId: {TraceId}, Type: {Type}", currentStream, eaiMessage.TraceId, eaiMessage.DataType);

                        using IDbConnection db = new NpgsqlConnection(auditConnString);
                        var auditRepo = new SqlAuditRepository(db);

                        await auditRepo.UpdateStatusAsync(eaiMessage.TraceId, MessageStatus.Processing);

                        // [특수 로직 처리]
                        if (eaiMessage.DataType == "AC_INFO")
                        {
                            // AC 케이스: 성공 처리 후 URL(Webhook) 호출
                            _logger.LogInformation(">>> [AC Flow] Calling Webhook URL: http://mock-target/api/accounting/confirm");
                            await Task.Delay(300); // 네트워크 지연 시뮬레이션
                            _logger.LogInformation(">>> [AC Flow] Webhook Call Success.");
                        }
                        else if (eaiMessage.DataType == "AR_INFO")
                        {
                            // AR 케이스: 성공 처리 후 다음 단계 프로듀서(또는 스트림) 호출
                            _logger.LogInformation(">>> [AR Flow] Triggering next step flow (Multi-hop)...");
                            var nextMessage = new EaiMessage
                            {
                                TraceId = TraceIdGenerator.Generate(),
                                IdempotencyKey = eaiMessage.IdempotencyKey + "_NEXT",
                                SourceSystem = "EAI_TRANSFORMER",
                                TargetSystem = "FINANCE_FINAL",
                                DataType = "AR_SETTLEMENT",
                                Payload = eaiMessage.Payload,
                                Timestamp = DateTime.UtcNow
                            };
                            await auditRepo.LogAsync(nextMessage, MessageStatus.Ready);
                            await publisher.PublishAsync("NEXT_STEP_STREAM", nextMessage);
                            _logger.LogInformation(">>> [AR Flow] Next step triggered. New TraceId: {NextTraceId}", nextMessage.TraceId);
                        }

                        await auditRepo.UpdateStatusAsync(eaiMessage.TraceId, MessageStatus.Success);
                        await consumer.AcknowledgeAsync(currentStream, groupName, entry.Id!);
                        _logger.LogInformation("[Success] TraceId: {TraceId} processed.", eaiMessage.TraceId);
                    }
                }
                catch (Exception ex) { _logger.LogError(ex, "Error in Stream {Stream}", currentStream); }
            }
            await Task.Delay(1000, stoppingToken);
        }
    }
}
