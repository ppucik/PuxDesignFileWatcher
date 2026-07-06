namespace PuxDesignFileWatcher.Infrastructure.Storage.Contracts;

/// <summary>
/// Serializable manifest snapshot contract used by storage repositories.
/// </summary>
public sealed class SerializableManifestSnapshot
{
    public string RootPath { get; set; } = string.Empty;

    public DateTimeOffset AnalyzedAtUtc { get; set; }

    public List<SerializableFileManifestEntry> Files { get; set; } = [];
}
