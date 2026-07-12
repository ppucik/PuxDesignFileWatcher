namespace PuxDesignFileWatcher.Domain.Changes;

/// <summary>
/// Represents one detected file change between snapshots.
/// </summary>
/// <param name="RelativePath">Path relative to analyzed root directory.</param>
/// <param name="ChangeType">Type of detected change.</param>
/// <param name="Version">Resulting file version associated with the change.</param>
/// <param name="EntryKind">Kind of changed entry (file or directory).</param>
public sealed record FileChangeRecord(
    string RelativePath,
    ChangeType ChangeType,
    int Version,
    ChangeEntryKind EntryKind = ChangeEntryKind.File);
