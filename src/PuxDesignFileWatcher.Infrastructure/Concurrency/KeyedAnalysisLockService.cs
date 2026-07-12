using System.Collections.Concurrent;
using PuxDesignFileWatcher.Application.Ports;

namespace PuxDesignFileWatcher.Infrastructure.Concurrency;

/// <summary>
/// Provides per-root asynchronous lock guarding concurrent analysis.
/// </summary>
public sealed class KeyedAnalysisLockService : IAnalysisLockPort
{
    private readonly ConcurrentDictionary<string, LockEntry> _locks = new(StringComparer.OrdinalIgnoreCase);

    /// <inheritdoc />
    public async Task<IAsyncDisposable> AcquireAsync(string rootPath, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(rootPath))
        {
            throw new ArgumentException("Root path must be provided.", nameof(rootPath));
        }

        var key = Path.GetFullPath(rootPath);
        var entry = _locks.GetOrAdd(key, _ => new LockEntry());
        Interlocked.Increment(ref entry.ReferenceCount);

        try
        {
            await entry.Semaphore.WaitAsync(cancellationToken);
            return new Releaser(this, key, entry);
        }
        catch
        {
            ReleaseEntry(key, entry, releaseSemaphore: false);
            throw;
        }
    }

    private void ReleaseEntry(string key, LockEntry entry, bool releaseSemaphore)
    {
        if (releaseSemaphore)
        {
            entry.Semaphore.Release();
        }

        if (Interlocked.Decrement(ref entry.ReferenceCount) != 0)
        {
            return;
        }

        if (_locks.TryRemove(new KeyValuePair<string, LockEntry>(key, entry)))
        {
            entry.Semaphore.Dispose();
        }
    }

    private sealed class LockEntry
    {
        public SemaphoreSlim Semaphore { get; } = new(1, 1);

        public int ReferenceCount;
    }

    private sealed class Releaser : IAsyncDisposable
    {
        private readonly KeyedAnalysisLockService _owner;
        private readonly string _key;
        private readonly LockEntry _entry;
        private int _disposed;

        public Releaser(KeyedAnalysisLockService owner, string key, LockEntry entry)
        {
            _owner = owner;
            _key = key;
            _entry = entry;
        }

        public ValueTask DisposeAsync()
        {
            if (Interlocked.Exchange(ref _disposed, 1) == 1)
            {
                return ValueTask.CompletedTask;
            }

            _owner.ReleaseEntry(_key, _entry, releaseSemaphore: true);
            return ValueTask.CompletedTask;
        }
    }
}
