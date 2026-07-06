using PuxDesignFileWatcher.Application.Abstractions.Cqrs;
using PuxDesignFileWatcher.Domain.Changes;
using PuxDesignFileWatcher.Domain.Services;

namespace PuxDesignFileWatcher.Application.UseCases.DiffState;

/// <summary>
/// Calculates differences between previous and current state.
/// </summary>
public sealed class DiffStateCommandHandler : ICommandHandler<DiffStateCommand, ManifestDiffResult>
{
    /// <inheritdoc />
    public Task<ManifestDiffResult> HandleAsync(DiffStateCommand command, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var result = ManifestDiffService.Diff(
            command.PreviousSnapshot,
            command.RootPath,
            command.CurrentScannedFiles,
            command.AnalyzedAtUtc);

        return Task.FromResult(result);
    }
}
