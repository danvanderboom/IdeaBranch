using System;
using System.Collections.Concurrent;

namespace CriticalInsight.Data.Agents;

public interface IRateLimiter
{
    bool TryConsume(string key, out TimeSpan? retryAfter);
}

public sealed class TokenBucketRateLimiter : IRateLimiter
{
    private sealed class Bucket
    {
        public double Tokens;
        public DateTime LastRefillUtc;
    }

    private readonly ConcurrentDictionary<string, Bucket> _buckets = new();
    private readonly int _capacity;
    private readonly double _tokensPerSecond;

    public TokenBucketRateLimiter(int capacity, TimeSpan refillPeriod)
    {
        _capacity = capacity;
        var seconds = refillPeriod.TotalSeconds;
        _tokensPerSecond = seconds > 0 ? capacity / seconds : double.MaxValue;
    }

    public bool TryConsume(string key, out TimeSpan? retryAfter)
    {
        var bucket = _buckets.GetOrAdd(key, _ => new Bucket { Tokens = _capacity, LastRefillUtc = DateTime.UtcNow });
        lock (bucket)
        {
            var now = DateTime.UtcNow;
            var elapsed = (now - bucket.LastRefillUtc).TotalSeconds;
            bucket.Tokens = Math.Min(_capacity, bucket.Tokens + elapsed * _tokensPerSecond);
            bucket.LastRefillUtc = now;

            if (bucket.Tokens >= 1.0)
            {
                bucket.Tokens -= 1.0;
                retryAfter = null;
                return true;
            }

            var needed = 1.0 - bucket.Tokens;
            var seconds = needed / _tokensPerSecond;
            retryAfter = TimeSpan.FromSeconds(Math.Max(0, seconds));
            return false;
        }
    }
}


