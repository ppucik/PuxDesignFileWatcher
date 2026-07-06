using PuxDesignFileWatcher.Application.Abstractions.Cqrs;
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
    private readonly IContentHashPort _contentHash;
    private readonly IQueryHandler<LoadStateQuery, DirectoryManifestSnapshot?> _loadState;
    private readonly ICommandHandler<DiffStateCommand, Domain.Changes.ManifestDiffResult> _diffState;
    private readonly ICommandHandler<SaveStateCommand, bool> _saveState;

    /// <summary>
    /// Initializes handler dependencies.
    /// </summary>
    public AnalyzeDirectoryCommandHandler(
        IAnalysisLockPort analysisLock,
        IFileSystemTraversalPort fileSystemTraversal,
        IContentHashPort contentHash,
        IQueryHandler<LoadStateQuery, DirectoryManifestSnapshot?> loadState,
        ICommandHandler<DiffStateCommand, Domain.Changes.ManifestDiffResult> diffState,
        ICommandHandler<SaveStateCommand, bool> saveState)
    {
        _analysisLock = analysisLock;
        _fileSystemTraversal = fileSystemTraversal;
        _contentHash = contentHash;
        _loadState = loadState;
        _diffState = diffState;
        _saveState = saveState;
    }

    /// <inheritdoc />
    public async Task<DirectoryAnalysisResult> HandleAsync(AnalyzeDirectoryCommand command, CancellationToken cancellationToken)
    {
        await using var lockHandle = await _analysisLock.AcquireAsync(command.RootPath, cancellationToken);

        var analyzedAtUtc = DateTimeOffset.UtcNow;
        var scannedFiles = await _fileSystemTraversal.EnumerateFilesAsync(command.RootPath, cancellationToken);

        var currentFiles = new Dictionary<string, FileManifestEntry>(StringComparer.OrdinalIgnoreCase);

        foreach (var scannedFile in scannedFiles)
        {
            var hash = await _contentHash.ComputeHashAsync(scannedFile.FullPath, cancellationToken);

            currentFiles[scannedFile.RelativePath] = new FileManifestEntry(
                scannedFile.RelativePath,
                scannedFile.SizeBytes,
                scannedFile.LastWriteTimeUtc,
                hash,
                version: 1);
        }

        var previousSnapshot = await _loadState.HandleAsync(
            new LoadStateQuery(command.RootPath),
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
