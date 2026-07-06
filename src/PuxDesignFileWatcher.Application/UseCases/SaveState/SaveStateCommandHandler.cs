using PuxDesignFileWatcher.Application.Abstractions.Cqrs;
using PuxDesignFileWatcher.Application.Ports;

namespace PuxDesignFileWatcher.Application.UseCases.SaveState;

/// <summary>
/// Persists the latest snapshot state.
/// </summary>
public sealed class SaveStateCommandHandler : ICommandHandler<SaveStateCommand, bool>
{
    private readonly IManifestPathResolverPort _manifestPathResolver;
    private readonly IManifestRepositoryPort _manifestRepository;

    /// <summary>
    /// Initializes handler dependencies.
    /// </summary>
    public SaveStateCommandHandler(
        IManifestPathResolverPort manifestPathResolver,
        IManifestRepositoryPort manifestRepository)
    {
        _manifestPathResolver = manifestPathResolver;
        _manifestRepository = manifestRepository;
    }

    /// <inheritdoc />
    public async Task<bool> HandleAsync(SaveStateCommand command, CancellationToken cancellationToken)
    {
        var manifestPath = _manifestPathResolver.ResolveManifestPath(command.RootPath);
        await _manifestRepository.SaveAsync(manifestPath, command.Snapshot, cancellationToken);
        return true;
    }
}
