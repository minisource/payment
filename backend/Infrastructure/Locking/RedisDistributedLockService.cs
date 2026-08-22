using Microsoft.Extensions.Logging;
using Minisource.Common.Locking;
using RedLockNet;
using RedLockNet.SERedis;
using RedLockNet.SERedis.Configuration;
using StackExchange.Redis;

namespace Infrastructure.Locking;

/// <summary>
/// Configuration options for Redis-based distributed locking.
/// </summary>
public class RedisLockOptions
{
    public const string SectionName = "RedisLock";

    /// <summary>
    /// Redis connection string.
    /// </summary>
    public string ConnectionString { get; set; } = "localhost:6379";

    /// <summary>
    /// Default lock expiry time.
    /// </summary>
    public TimeSpan DefaultExpiry { get; set; } = TimeSpan.FromSeconds(30);

    /// <summary>
    /// Default wait time to acquire lock.
    /// </summary>
    public TimeSpan DefaultWait { get; set; } = TimeSpan.FromSeconds(10);

    /// <summary>
    /// Default retry time between lock attempts.
    /// </summary>
    public TimeSpan DefaultRetry { get; set; } = TimeSpan.FromMilliseconds(200);

    /// <summary>
    /// Key prefix for all locks.
    /// </summary>
    public string KeyPrefix { get; set; } = "payment:lock:";
}

/// <summary>
/// Redis-based distributed lock service using RedLock.net.
/// </summary>
public class RedisDistributedLockService : IDistributedLockService, IDisposable
{
    private readonly RedLockFactory _redLockFactory;
    private readonly RedisLockOptions _options;
    private readonly ILogger<RedisDistributedLockService> _logger;
    private bool _disposed;

    public RedisDistributedLockService(
        IConnectionMultiplexer connectionMultiplexer,
        RedisLockOptions options,
        ILogger<RedisDistributedLockService> logger)
    {
        _options = options;
        _logger = logger;

        var multiplexers = new List<RedLockMultiplexer>
        {
            new(connectionMultiplexer)
        };

        _redLockFactory = RedLockFactory.Create(multiplexers);
    }

    /// <inheritdoc/>
    public async Task<IDistributedLock> AcquireAsync(
        string resource,
        TimeSpan expiry,
        CancellationToken cancellationToken = default)
    {
        var fullKey = $"{_options.KeyPrefix}{resource}";

        _logger.LogDebug("Attempting to acquire lock for resource: {Resource}", fullKey);

        var redLock = await _redLockFactory.CreateLockAsync(
            fullKey,
            expiry,
            _options.DefaultWait,
            _options.DefaultRetry,
            cancellationToken);

        _logger.LogDebug("Lock acquired: {Acquired} for resource: {Resource}", redLock.IsAcquired, fullKey);
        return new RedLockWrapper(redLock, fullKey, _logger);
    }

    /// <inheritdoc/>
    public async Task<IDistributedLock> AcquireAsync(
        string resource,
        TimeSpan expiry,
        TimeSpan wait,
        TimeSpan retry,
        CancellationToken cancellationToken = default)
    {
        var fullKey = $"{_options.KeyPrefix}{resource}";

        _logger.LogDebug("Attempting to acquire lock for resource: {Resource} with custom timeouts", fullKey);

        var redLock = await _redLockFactory.CreateLockAsync(
            fullKey,
            expiry,
            wait,
            retry,
            cancellationToken);

        _logger.LogDebug("Lock acquired: {Acquired} for resource: {Resource}", redLock.IsAcquired, fullKey);
        return new RedLockWrapper(redLock, fullKey, _logger);
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _redLockFactory.Dispose();
            _disposed = true;
        }
    }
}

/// <summary>
/// Wrapper around RedLock implementing IDistributedLock.
/// </summary>
internal class RedLockWrapper : IDistributedLock
{
    private readonly IRedLock _redLock;
    private readonly string _resource;
    private readonly ILogger _logger;
    private bool _disposed;

    public RedLockWrapper(IRedLock redLock, string resource, ILogger logger)
    {
        _redLock = redLock;
        _resource = resource;
        _logger = logger;
    }

    public string Resource => _resource;

    public bool IsAcquired => _redLock.IsAcquired && !_disposed;

    public Task<bool> ExtendAsync(TimeSpan extension)
    {
        // RedLock.net doesn't support extension directly
        // Return false to indicate extension is not supported
        _logger.LogWarning("Lock extension not supported for resource: {Resource}", _resource);
        return Task.FromResult(false);
    }

    public Task ReleaseAsync()
    {
        if (!_disposed)
        {
            _logger.LogDebug("Releasing lock for resource: {Resource}", _resource);
            _redLock.Dispose();
            _disposed = true;
        }
        return Task.CompletedTask;
    }

    public ValueTask DisposeAsync()
    {
        if (!_disposed)
        {
            _redLock.Dispose();
            _disposed = true;
        }
        return ValueTask.CompletedTask;
    }
}
