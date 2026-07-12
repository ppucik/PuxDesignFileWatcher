using System.Collections.Concurrent;
using PuxDesignFileWatcher.Application.Ports;

namespace PuxDesignFileWatcher.Infrastructure.Concurrency;

/// <summary>
/// Provides per-root asynchronous lock guarding concurrent analysis.
/// </summary>
public sealed class KeyedAnalysisLockService : IAnalysisLockPort
{
    private readonly ConcurrentDictionary<string, SemaphoreSlim> _locks = new(StringComparer.OrdinalIgnoreCase);

    /// <inheritdoc />
    public async Task<IAsyncDisposable> AcquireAsync(string rootPath, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(rootPath))
        {
            throw new ArgumentException("Root path must be provided.", nameof(rootPath));
        }

        var key = Path.GetFullPath(rootPath);
        var semaphore = _locks.GetOrAdd(key, _ => new SemaphoreSlim(1, 1));

        await semaphore.WaitAsync(cancellationToken);
        return new Releaser(semaphore);
    }

    private sealed class Releaser : IAsyncDisposable
    {
        private readonly SemaphoreSlim _semaphore;
        private int _disposed;

        public Releaser(SemaphoreSlim semaphore)
        {
            _semaphore = semaphore;
        }

        public ValueTask DisposeAsync()
        {
            if (Interlocked.Exchange(ref _disposed, 1) == 1)
            {
                return ValueTask.CompletedTask;
            }

            _semaphore.Release();
            return ValueTask.CompletedTask;
        }
    }
}
