using MessagePack;
using MessagePack.Resolvers;
using PuxDesignFileWatcher.Application.Ports;
using PuxDesignFileWatcher.Domain.Entities;
using PuxDesignFileWatcher.Infrastructure.Storage.Contracts;

namespace PuxDesignFileWatcher.Infrastructure.Storage;

/// <summary>
/// Persists manifest snapshots as MessagePack.
/// </summary>
public sealed class MessagePackManifestRepository : IManifestRepositoryPort
{
    private static readonly MessagePackSerializerOptions SerializerOptions = MessagePackSerializerOptions.Standard
        .WithResolver(ContractlessStandardResolver.Instance);

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

        var dto = await MessagePackSerializer.DeserializeAsync<SerializableManifestSnapshot>(stream, SerializerOptions, cancellationToken);
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

        await MessagePackSerializer.SerializeAsync(stream, dto, SerializerOptions, cancellationToken);
    }
}
