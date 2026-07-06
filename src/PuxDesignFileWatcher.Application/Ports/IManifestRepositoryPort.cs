using PuxDesignFileWatcher.Domain.Entities;

namespace PuxDesignFileWatcher.Application.Ports;

/// <summary>
/// Abstraction for loading and saving manifest snapshots.
/// </summary>
public interface IManifestRepositoryPort
{
    /// <summary>
    /// Loads a snapshot from storage.
    /// </summary>
    Task<DirectoryManifestSnapshot?> LoadAsync(string manifestPath, CancellationToken cancellationToken);

    /// <summary>
    /// Saves a snapshot to storage.
    /// </summary>
    Task SaveAsync(string manifestPath, DirectoryManifestSnapshot snapshot, CancellationToken cancellationToken);
}
