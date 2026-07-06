using Microsoft.Extensions.Logging;
using PuxDesignFileWatcher.Application.Models;
using PuxDesignFileWatcher.Application.Ports;
using PuxDesignFileWatcher.Domain.Entities;

namespace PuxDesignFileWatcher.Infrastructure.Analysis;

/// <summary>
/// Builds current state using metadata-first and hash-on-demand strategy.
/// </summary>
public sealed class HybridFileStateBuilder : IHybridFileStateBuilderPort
{
    private readonly IContentHashPort _contentHash;
    private readonly ILogger<HybridFileStateBuilder> _logger;

    public HybridFileStateBuilder(
        IContentHashPort contentHash,
        ILogger<HybridFileStateBuilder> logger)
    {
        _contentHash = contentHash;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyDictionary<string, FileManifestEntry>> BuildCurrentStateAsync(
        IReadOnlyCollection<ScannedFileDescriptor> scannedFiles,
        DirectoryManifestSnapshot? previousSnapshot,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(scannedFiles);

        var previousFiles = previousSnapshot?.Files
            ?? new Dictionary<string, FileManifestEntry>(StringComparer.OrdinalIgnoreCase);

        var result = new Dictionary<string, FileManifestEntry>(StringComparer.OrdinalIgnoreCase);

        foreach (var file in scannedFiles)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var hasPrevious = previousFiles.TryGetValue(file.RelativePath, out var previousEntry);
            var canReuseHash = hasPrevious
                && previousEntry is not null
                && previousEntry.SizeBytes == file.SizeBytes
                && previousEntry.LastWriteTimeUtc == file.LastWriteTimeUtc;

            if (canReuseHash)
            {
                result[file.RelativePath] = new FileManifestEntry(
                    file.RelativePath,
                    file.SizeBytes,
                    file.LastWriteTimeUtc,
                    previousEntry!.ContentHash,
                    previousEntry.Version);
                continue;
            }

            try
            {
                var hash = await _contentHash.ComputeHashAsync(file.FullPath, cancellationToken);
                result[file.RelativePath] = new FileManifestEntry(
                    file.RelativePath,
                    file.SizeBytes,
                    file.LastWriteTimeUtc,
                    hash,
                    hasPrevious ? previousEntry!.Version : 1);
            }
            catch (IOException ex)
            {
                _logger.LogWarning(ex, "Skipping locked file during hash computation: {FilePath}", file.FullPath);

                if (hasPrevious && previousEntry is not null)
                {
                    result[file.RelativePath] = previousEntry;
                }
            }
        }

        return result;
    }
}
