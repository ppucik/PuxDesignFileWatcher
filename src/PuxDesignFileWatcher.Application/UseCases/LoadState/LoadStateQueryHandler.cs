using PuxDesignFileWatcher.Application.Abstractions.Cqrs;
using PuxDesignFileWatcher.Application.Ports;
using PuxDesignFileWatcher.Domain.Entities;

namespace PuxDesignFileWatcher.Application.UseCases.LoadState;

/// <summary>
/// Loads previously persisted state for a root path.
/// </summary>
public sealed class LoadStateQueryHandler : IQueryHandler<LoadStateQuery, DirectoryManifestSnapshot?>
{
    private readonly IManifestPathResolverPort _manifestPathResolver;
    private readonly IManifestRepositoryPort _manifestRepository;

    /// <summary>
    /// Initializes handler dependencies.
    /// </summary>
    public LoadStateQueryHandler(
        IManifestPathResolverPort manifestPathResolver,
        IManifestRepositoryPort manifestRepository)
    {
        _manifestPathResolver = manifestPathResolver;
        _manifestRepository = manifestRepository;
    }

    /// <inheritdoc />
    public Task<DirectoryManifestSnapshot?> HandleAsync(LoadStateQuery query, CancellationToken cancellationToken)
    {
        var manifestPath = _manifestPathResolver.ResolveManifestPath(query.RootPath);
        return _manifestRepository.LoadAsync(manifestPath, cancellationToken);
    }
}
