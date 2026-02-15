using Eai.Shared.Models;
using StackExchange.Redis;
using System.Text.Json;

namespace Eai.Infrastructure.Redis;

/// <summary>
/// Redis Streams에 메시지를 발행하는 역할을 담당하는 서비스입니다.
/// </summary>
public class RedisStreamPublisher
{
    private readonly IConnectionMultiplexer _redis;
    private readonly IDatabase _db;

    public RedisStreamPublisher(IConnectionMultiplexer redis)
    {
        _redis = redis;
        _db = _redis.GetDatabase();
    }

    /// <summary>
    /// EaiMessage 객체를 JSON으로 직렬화하여 지정된 스트림에 발행합니다.
    /// </summary>
    /// <param name="streamName">발행할 Redis 스트림 이름</param>
    /// <param name="message">발행할 EAI 메시지 객체</param>
    /// <param name="maxLen">스트림의 최대 유지 길이 (보존 정책)</param>
    public async Task<string> PublishAsync(string streamName, EaiMessage message, int maxLen = 100000)
    {
        // 1. 메시지 객체를 JSON 문자열로 변환
        string payload = JsonSerializer.Serialize(message);

        // 2. Redis Stream에 추가할 데이터 구성
        var entries = new NameValueEntry[]
        {
            new("traceId", message.TraceId),
            new("data", payload)
        };

        // 3. XADD 명령 실행 (MAXLEN 설정을 통해 스트림 크기 제한)
        // 결과값으로 Redis가 생성한 메시지 ID(예: 1625...-0)를 반환합니다.
        return (await _db.StreamAddAsync(streamName, entries, maxLength: maxLen, useApproximateMaxLength: true))!;
    }
}
