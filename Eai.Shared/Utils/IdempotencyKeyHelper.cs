namespace Eai.Shared.Utils;

public static class IdempotencyKeyHelper
{
    public static string Create(string sourcePk, string transactionId)
    {
        return $"{sourcePk}:{transactionId}";
    }
}
