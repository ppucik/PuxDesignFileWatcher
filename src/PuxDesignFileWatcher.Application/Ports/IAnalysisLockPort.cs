namespace PuxDesignFileWatcher.Application.Ports;

/// <summary>
/// Abstraction for per-root analysis concurrency guard.
/// </summary>
public interface IAnalysisLockPort
{
    /// <summary>
    /// Acquires an asynchronous lock for the provided root path.
    /// </summary>
    Task<IAsyncDisposable> AcquireAsync(string rootPath, CancellationToken cancellationToken);
}
