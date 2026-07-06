using PuxDesignFileWatcher.Application.Models;

namespace PuxDesignFileWatcher.Application.Ports;

/// <summary>
/// Abstraction for recursive filesystem traversal.
/// </summary>
public interface IFileSystemTraversalPort
{
    /// <summary>
    /// Recursively enumerates all files under the provided root path.
    /// </summary>
    Task<IReadOnlyCollection<ScannedFileDescriptor>> EnumerateFilesAsync(
        string rootPath,
        CancellationToken cancellationToken);
}
