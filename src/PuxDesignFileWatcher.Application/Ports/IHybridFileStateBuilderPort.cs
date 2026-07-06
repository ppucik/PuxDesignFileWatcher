using PuxDesignFileWatcher.Application.Models;
using PuxDesignFileWatcher.Domain.Entities;

namespace PuxDesignFileWatcher.Application.Ports;

/// <summary>
/// Builds current file state using hybrid detection strategy.
/// </summary>
public interface IHybridFileStateBuilderPort
{
    /// <summary>
    /// Builds current file states keyed by relative path.
    /// </summary>
    Task<IReadOnlyDictionary<string, FileManifestEntry>> BuildCurrentStateAsync(
        IReadOnlyCollection<ScannedFileDescriptor> scannedFiles,
        DirectoryManifestSnapshot? previousSnapshot,
        CancellationToken cancellationToken);
}
