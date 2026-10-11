using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using TaskFlow.Application.Identity;

namespace TaskFlow.Infrastructure.Identity;

/// <summary>Counts the failed sign-ins of each address in memory, for a window that starts at the first failure.</summary>
internal sealed class SignInAttemptLimiter(IMemoryCache memoryCache, IOptions<RateLimitingSettings> rateLimitingSettings)
    : ISignInAttemptLimiter
{
    public bool IsLimited(string userName)
    {
        return memoryCache.TryGetValue(KeyFor(userName), out FailedSignIns? failedSignIns)
               && failedSignIns!.Count >= rateLimitingSettings.Value.FailedSignInsPerAddress;
    }

    public void RecordFailure(string userName)
    {
        var failedSignIns = memoryCache.GetOrCreate(KeyFor(userName), entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = rateLimitingSettings.Value.FailedSignInWindow;
            return new FailedSignIns();
        });

        failedSignIns!.Increment();
    }

    public void Reset(string userName)
    {
        memoryCache.Remove(KeyFor(userName));
    }

    private static string KeyFor(string userName)
    {
        return $"FailedSignIns_{userName}";
    }

    private sealed class FailedSignIns
    {
        private int _count;

        public int Count => _count;

        public void Increment()
        {
            Interlocked.Increment(ref _count);
        }
    }
}
