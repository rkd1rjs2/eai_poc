using Eai.Shared.Constants;
using Eai.Shared.Models;
using StackExchange.Redis;
using System.Text.Json;

namespace Eai.Infrastructure.Redis;

/// <summary>
/// Redis Streams에서 소비자 그룹을 통해 메시지를 읽어오는 역할을 담당하는 서비스입니다.
/// </summary>
public class RedisStreamConsumer
{
    private readonly IConnectionMultiplexer _redis;
    private readonly IDatabase _db;

    public RedisStreamConsumer(IConnectionMultiplexer redis)
    {
        _redis = redis;
        _db = _redis.GetDatabase();
    }

    /// <summary>
    /// 소비자 그룹을 생성합니다. (이미 존재하는 경우 무시)
    /// </summary>
    public async Task CreateConsumerGroupAsync(string streamName, string groupName)
    {
        try
        {
            // 스트림이 없으면 생성하고($는 현재 시점 이후부터 읽겠다는 의미) 그룹을 만듭니다.
            await _db.StreamCreateConsumerGroupAsync(streamName, groupName, StreamPosition.NewMessages);
        }
        catch (RedisServerException ex) when (ex.Message.Contains("BUSYGROUP"))
        {
            // 이미 그룹이 존재하는 경우 발생하는 예외이므로 무시합니다.
        }
    }

    /// <summary>
    /// 스트림에서 메시지를 읽어옵니다.
    /// </summary>
    public async Task<StreamEntry[]> ReadMessagesAsync(string streamName, string groupName, string consumerName, int count = 10)
    {
        // 소비자 그룹을 통해 읽지 않은 메시지(>)를 가져옵니다.
        return await _db.StreamReadGroupAsync(streamName, groupName, consumerName, ">", count);
    }

    /// <summary>
    /// 메시지 처리가 완료되었음을 Redis에 알립니다. (Acknowledge)
    /// </summary>
    public async Task AcknowledgeAsync(string streamName, string groupName, string messageId)
    {
        await _db.StreamAcknowledgeAsync(streamName, groupName, messageId);
    }
}
