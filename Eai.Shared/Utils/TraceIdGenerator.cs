namespace Eai.Shared.Utils;

public static class TraceIdGenerator
{
    public static string Generate() => Guid.NewGuid().ToString("N");
}
