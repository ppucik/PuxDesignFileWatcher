using PuxDesignFileWatcher.Application.Abstractions.CQRS;
using PuxDesignFileWatcher.Application.Ports;
using PuxDesignFileWatcher.Application.UseCases.DiffState;
using PuxDesignFileWatcher.Application.UseCases.LoadState;
using PuxDesignFileWatcher.Application.UseCases.SaveState;
using PuxDesignFileWatcher.Domain.Entities;

namespace PuxDesignFileWatcher.Application.UseCases.AnalyzeDirectory;

/// <summary>
/// Orchestrates full directory analysis workflow.
/// </summary>
public sealed class AnalyzeDirectoryCommandHandler : ICommandHandler<AnalyzeDirectoryCommand, DirectoryAnalysisResult>
{
    private readonly IAnalysisLockPort _analysisLock;
    private readonly IFileSystemTraversalPort _fileSystemTraversal;
    private readonly IHybridFileStateBuilderPort _hybridFileStateBuilder;
    private readonly IQueryHandler<LoadStateQuery, DirectoryManifestSnapshot?> _loadState;
    private readonly ICommandHandler<DiffStateCommand, Domain.Changes.ManifestDiffResult> _diffState;
    private readonly ICommandHandler<SaveStateCommand, bool> _saveState;

    /// <summary>
    /// Initializes handler dependencies.
    /// </summary>
    public AnalyzeDirectoryCommandHandler(
        IAnalysisLockPort analysisLock,
        IFileSystemTraversalPort fileSystemTraversal,
        IHybridFileStateBuilderPort hybridFileStateBuilder,
        IQueryHandler<LoadStateQuery, DirectoryManifestSnapshot?> loadState,
        ICommandHandler<DiffStateCommand, Domain.Changes.ManifestDiffResult> diffState,
        ICommandHandler<SaveStateCommand, bool> saveState)
    {
        _analysisLock = analysisLock;
        _fileSystemTraversal = fileSystemTraversal;
        _hybridFileStateBuilder = hybridFileStateBuilder;
        _loadState = loadState;
        _diffState = diffState;
        _saveState = saveState;
    }

    /// <inheritdoc />
    public async Task<DirectoryAnalysisResult> HandleAsync(AnalyzeDirectoryCommand command, CancellationToken cancellationToken)
    {
        await using var lockHandle = await _analysisLock.AcquireAsync(command.RootPath, cancellationToken);

        var analyzedAtUtc = DateTimeOffset.UtcNow;
        var previousSnapshot = await _loadState.HandleAsync(
            new LoadStateQuery(command.RootPath),
            cancellationToken);

        var scannedFiles = await _fileSystemTraversal.EnumerateFilesAsync(command.RootPath, cancellationToken);
        var currentFiles = await _hybridFileStateBuilder.BuildCurrentStateAsync(
            scannedFiles,
            previousSnapshot,
            cancellationToken);

        var diffResult = await _diffState.HandleAsync(
            new DiffStateCommand(command.RootPath, previousSnapshot, currentFiles, analyzedAtUtc),
            cancellationToken);

        await _saveState.HandleAsync(
            new SaveStateCommand(command.RootPath, diffResult.NextSnapshot),
            cancellationToken);

        return new DirectoryAnalysisResult(
            command.RootPath,
            analyzedAtUtc,
            diffResult.Changes,
            diffResult.NextSnapshot);
    }
}
