namespace PuxDesignFileWatcher.Infrastructure.Configuration;

/// <summary>
/// Supported manifest storage formats.
/// </summary>
public enum StorageFormat
{
    /// <summary>
    /// Store manifests in JSON format.
    /// </summary>
    Json = 1,

    /// <summary>
    /// Store manifests in MessagePack format.
    /// </summary>
    MessagePack = 2
}
