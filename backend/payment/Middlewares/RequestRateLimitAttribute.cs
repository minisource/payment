using System.Collections.Concurrent;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Presentaion.Middlewares;

/// <summary>
/// Simple in-memory rate limiter for admin endpoints.
/// Tracks request counts per IP in a sliding window.
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class RequestRateLimitAttribute : ActionFilterAttribute
{
    private static readonly ConcurrentDictionary<string, SlidingWindow> _counters = new();

    public int MaxRequests { get; set; } = 10;
    public int WindowSeconds { get; set; } = 60;

    public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var ip = context.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var key = $"{context.ActionDescriptor.DisplayName}:{ip}";

        var window = _counters.GetOrAdd(key, _ => new SlidingWindow(WindowSeconds));
        if (!window.TryIncrement(MaxRequests))
        {
            context.Result = new ObjectResult(new
            {
                error = "rate_limit_exceeded",
                message = $"Too many requests. Max {MaxRequests} per {WindowSeconds}s.",
                retry_after_seconds = window.ResetInSeconds()
            })
            {
                StatusCode = 429
            };
            return;
        }

        // Periodic cleanup: remove stale entries older than 2x window
        PruneStaleEntries();

        await next();
    }

    private static void PruneStaleEntries()
    {
        // Run cleanup at most every 2 minutes
        var now = DateTime.UtcNow;
        if ((now - _lastPrune).TotalMinutes < 2) return;
        _lastPrune = now;

        var toRemove = new List<string>();
        foreach (var kvp in _counters)
        {
            if (kvp.Value.IsExpired(now))
                toRemove.Add(kvp.Key);
        }
        foreach (var key in toRemove)
            _counters.TryRemove(key, out _);
    }

    private static DateTime _lastPrune = DateTime.UtcNow;

    private class SlidingWindow
    {
        private readonly int _windowSeconds;
        private readonly object _lock = new();
        private readonly Queue<DateTime> _timestamps = new();
        private DateTime _windowStart = DateTime.UtcNow;

        public SlidingWindow(int windowSeconds) => _windowSeconds = windowSeconds;

        public bool TryIncrement(int maxRequests)
        {
            lock (_lock)
            {
                var now = DateTime.UtcNow;
                // Reset window if expired
                if ((now - _windowStart).TotalSeconds > _windowSeconds)
                {
                    _timestamps.Clear();
                    _windowStart = now;
                }

                // Remove timestamps outside the window
                while (_timestamps.Count > 0 && (now - _timestamps.Peek()).TotalSeconds > _windowSeconds)
                    _timestamps.Dequeue();

                if (_timestamps.Count >= maxRequests)
                    return false;

                _timestamps.Enqueue(now);
                return true;
            }
        }

        public int ResetInSeconds()
        {
            lock (_lock)
            {
                if (_timestamps.Count == 0) return 0;
                var elapsed = (DateTime.UtcNow - _timestamps.Peek()).TotalSeconds;
                return Math.Max(0, (int)(_windowSeconds - elapsed) + 1);
            }
        }

        public bool IsExpired(DateTime now)
        {
            lock (_lock)
            {
                if (_timestamps.Count == 0) return true;
                return (now - _timestamps.Peek()).TotalSeconds > _windowSeconds * 2;
            }
        }
    }
}
