using System.Text.Json;
using PuxDesignFileWatcher.Application.Ports;
using PuxDesignFileWatcher.Domain.Entities;
using PuxDesignFileWatcher.Infrastructure.Storage.Contracts;

namespace PuxDesignFileWatcher.Infrastructure.Storage;

/// <summary>
/// Persists manifest snapshots as JSON.
/// </summary>
public sealed class JsonManifestRepository : IManifestRepositoryPort
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true
    };

    /// <inheritdoc />
    public async Task<DirectoryManifestSnapshot?> LoadAsync(string manifestPath, CancellationToken cancellationToken)
    {
        if (!File.Exists(manifestPath))
        {
            return null;
        }

        await using var stream = new FileStream(
            manifestPath,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read,
            bufferSize: 1024 * 8,
            FileOptions.Asynchronous | FileOptions.SequentialScan);

        var dto = await JsonSerializer.DeserializeAsync<SerializableManifestSnapshot>(stream, SerializerOptions, cancellationToken);
        return dto is null ? null : ManifestSnapshotMapper.ToDomain(dto);
    }

    /// <inheritdoc />
    public async Task SaveAsync(string manifestPath, DirectoryManifestSnapshot snapshot, CancellationToken cancellationToken)
    {
        var dto = ManifestSnapshotMapper.ToSerializable(snapshot);
        var directoryPath = Path.GetDirectoryName(manifestPath);

        if (!string.IsNullOrWhiteSpace(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }

        await using var stream = new FileStream(
            manifestPath,
            FileMode.Create,
            FileAccess.Write,
            FileShare.None,
            bufferSize: 1024 * 8,
            FileOptions.Asynchronous);

        await JsonSerializer.SerializeAsync(stream, dto, SerializerOptions, cancellationToken);
    }
}
