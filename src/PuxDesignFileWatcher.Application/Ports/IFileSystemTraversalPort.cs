using PuxDesignFileWatcher.Application.Models;

namespace PuxDesignFileWatcher.Application.Ports;

/// <summary>
/// Abstraction for recursive filesystem traversal.
/// </summary>
public interface IFileSystemTraversalPort
{
    /// <summary>
    /// Recursively enumerates files and subdirectories under the provided root path.
    /// </summary>
    Task<ScannedDirectorySnapshot> EnumerateFilesAsync(
        string rootPath,
        CancellationToken cancellationToken);
}
