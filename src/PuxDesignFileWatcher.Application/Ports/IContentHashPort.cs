namespace PuxDesignFileWatcher.Application.Ports;

/// <summary>
/// Abstraction for computing content hash of files.
/// </summary>
public interface IContentHashPort
{
    /// <summary>
    /// Computes deterministic content hash for a file.
    /// </summary>
    Task<string> ComputeHashAsync(string filePath, CancellationToken cancellationToken);
}
