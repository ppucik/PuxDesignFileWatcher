using PuxDesignFileWatcher.Domain.Entities;

namespace PuxDesignFileWatcher.Infrastructure.Storage.Contracts;

/// <summary>
/// Maps between domain snapshot model and serializable storage contracts.
/// </summary>
internal static class ManifestSnapshotMapper
{
    public static SerializableManifestSnapshot ToSerializable(DirectoryManifestSnapshot snapshot)
    {
        return new SerializableManifestSnapshot
        {
            RootPath = snapshot.RootPath,
            AnalyzedAtUtc = snapshot.AnalyzedAtUtc,
            Files = snapshot.Files.Values
                .Select(file => new SerializableFileManifestEntry
                {
                    RelativePath = file.RelativePath,
                    SizeBytes = file.SizeBytes,
                    LastWriteTimeUtc = file.LastWriteTimeUtc,
                    ContentHash = file.ContentHash,
                    Version = file.Version
                })
                .ToList()
        };
    }

    public static DirectoryManifestSnapshot ToDomain(SerializableManifestSnapshot snapshot)
    {
        var files = snapshot.Files.ToDictionary(
            file => file.RelativePath,
            file => new FileManifestEntry(
                file.RelativePath,
                file.SizeBytes,
                file.LastWriteTimeUtc,
                file.ContentHash,
                file.Version),
            StringComparer.OrdinalIgnoreCase);

        return new DirectoryManifestSnapshot(snapshot.RootPath, snapshot.AnalyzedAtUtc, files);
    }
}
