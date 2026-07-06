namespace PuxDesignFileWatcher.Infrastructure.Storage.Contracts;

/// <summary>
/// Serializable file entry contract used by storage repositories.
/// </summary>
public sealed class SerializableFileManifestEntry
{
    public string RelativePath { get; set; } = string.Empty;

    public long SizeBytes { get; set; }

    public DateTimeOffset LastWriteTimeUtc { get; set; }

    public string ContentHash { get; set; } = string.Empty;

    public int Version { get; set; }
}
