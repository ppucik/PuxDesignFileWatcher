using PuxDesignFileWatcher.Application.Ports;
using PuxDesignFileWatcher.Domain.Entities;
using PuxDesignFileWatcher.Infrastructure.Configuration;

namespace PuxDesignFileWatcher.Infrastructure.Storage;

/// <summary>
/// Selects JSON or MessagePack repository based on configured storage format.
/// </summary>
public sealed class ConfigurableManifestRepository : IManifestRepositoryPort
{
    private readonly StorageOptionsAccessor _storageOptions;
    private readonly JsonManifestRepository _jsonRepository;
    private readonly MessagePackManifestRepository _messagePackRepository;

    public ConfigurableManifestRepository(
        StorageOptionsAccessor storageOptions,
        JsonManifestRepository jsonRepository,
        MessagePackManifestRepository messagePackRepository)
    {
        _storageOptions = storageOptions;
        _jsonRepository = jsonRepository;
        _messagePackRepository = messagePackRepository;
    }

    /// <inheritdoc />
    public Task<DirectoryManifestSnapshot?> LoadAsync(string manifestPath, CancellationToken cancellationToken)
        => GetRepository().LoadAsync(manifestPath, cancellationToken);

    /// <inheritdoc />
    public Task SaveAsync(string manifestPath, DirectoryManifestSnapshot snapshot, CancellationToken cancellationToken)
        => GetRepository().SaveAsync(manifestPath, snapshot, cancellationToken);

    private IManifestRepositoryPort GetRepository()
        => _storageOptions.StorageFormat == StorageFormat.MessagePack
            ? _messagePackRepository
            : _jsonRepository;
}
