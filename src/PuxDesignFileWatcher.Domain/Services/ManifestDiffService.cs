using PuxDesignFileWatcher.Domain.Changes;
using PuxDesignFileWatcher.Domain.Entities;

namespace PuxDesignFileWatcher.Domain.Services;

/// <summary>
/// Provides pure domain logic for diffing two snapshots and computing file versions.
/// </summary>
public static class ManifestDiffService
{
    /// <summary>
    /// Compares the previous snapshot with current scanned files and computes changes.
    /// </summary>
    /// <param name="previousSnapshot">Previous snapshot or <see langword="null"/> for first run.</param>
    /// <param name="rootPath">Analyzed root directory path.</param>
    /// <param name="currentScannedFiles">
    /// Current file states keyed by relative path. Versions in this input are ignored and recomputed.
    /// </param>
    /// <param name="analyzedAtUtc">Timestamp representing analysis completion in UTC.</param>
    /// <returns>Diff result with computed next snapshot and detected changes.</returns>
    public static ManifestDiffResult Diff(
        DirectoryManifestSnapshot? previousSnapshot,
        string rootPath,
        IReadOnlyDictionary<string, FileManifestEntry> currentScannedFiles,
        DateTimeOffset analyzedAtUtc)
    {
        if (string.IsNullOrWhiteSpace(rootPath))
        {
            throw new ArgumentException("Root path must be provided.", nameof(rootPath));
        }

        ArgumentNullException.ThrowIfNull(currentScannedFiles);

        var previousFiles = previousSnapshot?.Files
            ?? new Dictionary<string, FileManifestEntry>(StringComparer.OrdinalIgnoreCase);

        var nextFiles = new Dictionary<string, FileManifestEntry>(StringComparer.OrdinalIgnoreCase);
        var changes = new List<FileChangeRecord>();

        foreach (var (relativePath, current) in currentScannedFiles)
        {
            if (!previousFiles.TryGetValue(relativePath, out var previous))
            {
                var newEntry = FileManifestEntry.CreateNew(
                    relativePath,
                    current.SizeBytes,
                    current.LastWriteTimeUtc,
                    current.ContentHash);

                nextFiles[relativePath] = newEntry;
                changes.Add(new FileChangeRecord(relativePath, ChangeType.New, newEntry.Version));
                continue;
            }

            var hasContentChanged = !string.Equals(
                previous.ContentHash,
                current.ContentHash,
                StringComparison.Ordinal);

            var nextVersion = hasContentChanged ? previous.Version + 1 : previous.Version;

            var nextEntry = new FileManifestEntry(
                relativePath,
                current.SizeBytes,
                current.LastWriteTimeUtc,
                current.ContentHash,
                nextVersion);

            nextFiles[relativePath] = nextEntry;

            if (hasContentChanged)
            {
                changes.Add(new FileChangeRecord(relativePath, ChangeType.Changed, nextVersion));
            }
        }

        foreach (var (relativePath, previous) in previousFiles)
        {
            if (currentScannedFiles.ContainsKey(relativePath))
            {
                continue;
            }

            changes.Add(new FileChangeRecord(relativePath, ChangeType.Deleted, previous.Version));
        }

        var nextSnapshot = new DirectoryManifestSnapshot(rootPath, analyzedAtUtc, nextFiles);

        return new ManifestDiffResult(nextSnapshot, changes);
    }
}
