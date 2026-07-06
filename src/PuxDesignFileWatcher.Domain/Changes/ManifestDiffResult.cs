using PuxDesignFileWatcher.Domain.Entities;

namespace PuxDesignFileWatcher.Domain.Changes;

/// <summary>
/// Represents the result of comparing two manifest snapshots.
/// </summary>
/// <param name="NextSnapshot">Snapshot with correctly computed versions for current files.</param>
/// <param name="Changes">Collection of detected file changes.</param>
public sealed record ManifestDiffResult(
    DirectoryManifestSnapshot NextSnapshot,
    IReadOnlyCollection<FileChangeRecord> Changes);
