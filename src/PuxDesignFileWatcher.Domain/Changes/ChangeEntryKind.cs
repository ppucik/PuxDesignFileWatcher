namespace PuxDesignFileWatcher.Domain.Changes;

/// <summary>
/// Represents kind of entry related to detected change.
/// </summary>
public enum ChangeEntryKind
{
    /// <summary>
    /// Change belongs to a file entry.
    /// </summary>
    File = 1,

    /// <summary>
    /// Change belongs to a directory entry.
    /// </summary>
    Directory = 2
}
