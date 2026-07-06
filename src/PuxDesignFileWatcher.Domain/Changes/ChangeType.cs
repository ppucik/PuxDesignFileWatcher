namespace PuxDesignFileWatcher.Domain.Changes;

/// <summary>
/// Represents file change types detected between two snapshots.
/// </summary>
public enum ChangeType
{
    /// <summary>
    /// A file exists in the current snapshot but not in the previous snapshot.
    /// </summary>
    New = 1,

    /// <summary>
    /// A file exists in both snapshots and its content changed.
    /// </summary>
    Changed = 2,

    /// <summary>
    /// A file exists in the previous snapshot but not in the current snapshot.
    /// </summary>
    Deleted = 3
}
