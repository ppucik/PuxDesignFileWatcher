using PuxDesignFileWatcher.Application.Abstractions.CQRS;
using PuxDesignFileWatcher.Application.Models;
using PuxDesignFileWatcher.Application.Ports;
using PuxDesignFileWatcher.Application.UseCases.AnalyzeDirectory;
using PuxDesignFileWatcher.Application.UseCases.DiffState;
using PuxDesignFileWatcher.Application.UseCases.LoadState;
using PuxDesignFileWatcher.Application.UseCases.SaveState;
using PuxDesignFileWatcher.Domain.Changes;
using PuxDesignFileWatcher.Domain.Entities;

namespace PuxDesignFileWatcher.UnitTests.Application;

public sealed class AnalyzeDirectoryCommandHandlerLimitTests : IAsyncLifetime
{
    private string _rootPath = string.Empty;

    [Fact]
    public async Task HandleAsync_WhenFileCountExceedsLimit_ShouldThrowValidationException()
    {
        var files = Enumerable.Range(1, 3)
            .Select(i => new ScannedFileDescriptor(Path.Combine(_rootPath, $"f{i}.txt"), $"f{i}.txt", 10, DateTimeOffset.UtcNow))
            .ToArray();

        var handler = CreateHandler(
            scannedSnapshot: new ScannedDirectorySnapshot(files, []),
            limits: new AnalysisLimitsSettings(MaxFileCount: 2, MaxFileSizeBytes: 1024 * 1024));

        var ex = await Assert.ThrowsAsync<DirectoryAnalysisValidationException>(() =>
            handler.HandleAsync(new AnalyzeDirectoryCommand(_rootPath), CancellationToken.None));

        Assert.Contains("exceeds the configured limit", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task HandleAsync_WhenSingleFileExceedsSizeLimit_ShouldThrowValidationException()
    {
        var files = new[]
        {
            new ScannedFileDescriptor(Path.Combine(_rootPath, "big.bin"), "big.bin", 6 * 1024 * 1024, DateTimeOffset.UtcNow)
        };

        var handler = CreateHandler(
            scannedSnapshot: new ScannedDirectorySnapshot(files, []),
            limits: new AnalysisLimitsSettings(MaxFileCount: 10, MaxFileSizeBytes: 5 * 1024 * 1024));

        var ex = await Assert.ThrowsAsync<DirectoryAnalysisValidationException>(() =>
            handler.HandleAsync(new AnalyzeDirectoryCommand(_rootPath), CancellationToken.None));

        Assert.Contains("big.bin", ex.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("exceeds the configured limit", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    public Task InitializeAsync()
    {
        _rootPath = Path.Combine(Path.GetTempPath(), "PuxDesignFileWatcherLimitTests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_rootPath);
        return Task.CompletedTask;
    }

    public Task DisposeAsync()
    {
        if (Directory.Exists(_rootPath))
        {
            Directory.Delete(_rootPath, recursive: true);
        }

        return Task.CompletedTask;
    }

    private AnalyzeDirectoryCommandHandler CreateHandler(ScannedDirectorySnapshot scannedSnapshot, AnalysisLimitsSettings limits)
    {
        var analysisLock = new NoOpAnalysisLockPort();
        var limitsPort = new FixedAnalysisLimitsPort(limits);
        var traversal = new FixedTraversalPort(scannedSnapshot);
        var hybridBuilder = new FixedHybridBuilderPort();
        var loadState = new FixedLoadStateHandler();
        var diffState = new FixedDiffStateHandler();
        var saveState = new FixedSaveStateHandler();

        return new AnalyzeDirectoryCommandHandler(
            analysisLock,
            limitsPort,
            traversal,
            hybridBuilder,
            loadState,
            diffState,
            saveState);
    }

    private sealed class FixedTraversalPort : IFileSystemTraversalPort
    {
        private readonly ScannedDirectorySnapshot _snapshot;

        public FixedTraversalPort(ScannedDirectorySnapshot snapshot)
        {
            _snapshot = snapshot;
        }

        public Task<ScannedDirectorySnapshot> EnumerateFilesAsync(string rootPath, CancellationToken cancellationToken)
            => Task.FromResult(_snapshot);
    }

    private sealed class FixedAnalysisLimitsPort : IAnalysisLimitsPort
    {
        private readonly AnalysisLimitsSettings _limits;

        public FixedAnalysisLimitsPort(AnalysisLimitsSettings limits)
        {
            _limits = limits;
        }

        public AnalysisLimitsSettings GetLimits() => _limits;
    }

    private sealed class FixedHybridBuilderPort : IHybridFileStateBuilderPort
    {
        public Task<IReadOnlyDictionary<string, FileManifestEntry>> BuildCurrentStateAsync(
            IReadOnlyCollection<ScannedFileDescriptor> scannedFiles,
            DirectoryManifestSnapshot? previousSnapshot,
            CancellationToken cancellationToken)
        {
            var dict = scannedFiles.ToDictionary(
                file => file.RelativePath,
                file => new FileManifestEntry(file.RelativePath, file.SizeBytes, file.LastWriteTimeUtc, "HASH", 1),
                StringComparer.OrdinalIgnoreCase);

            return Task.FromResult<IReadOnlyDictionary<string, FileManifestEntry>>(dict);
        }
    }

    private sealed class FixedLoadStateHandler : IQueryHandler<LoadStateQuery, DirectoryManifestSnapshot?>
    {
        public Task<DirectoryManifestSnapshot?> HandleAsync(LoadStateQuery query, CancellationToken cancellationToken)
            => Task.FromResult<DirectoryManifestSnapshot?>(null);
    }

    private sealed class FixedDiffStateHandler : ICommandHandler<DiffStateCommand, ManifestDiffResult>
    {
        public Task<ManifestDiffResult> HandleAsync(DiffStateCommand command, CancellationToken cancellationToken)
        {
            var next = command.PreviousSnapshot ?? new DirectoryManifestSnapshot(command.RootPath, command.AnalyzedAtUtc, new Dictionary<string, FileManifestEntry>());
            return Task.FromResult(new ManifestDiffResult(next, []));
        }
    }

    private sealed class FixedSaveStateHandler : ICommandHandler<SaveStateCommand, bool>
    {
        public Task<bool> HandleAsync(SaveStateCommand command, CancellationToken cancellationToken)
            => Task.FromResult(true);
    }

    private sealed class NoOpAnalysisLockPort : IAnalysisLockPort
    {
        public Task<IAsyncDisposable> AcquireAsync(string rootPath, CancellationToken cancellationToken)
            => Task.FromResult<IAsyncDisposable>(new AsyncDisposableStub());
    }

    private sealed class AsyncDisposableStub : IAsyncDisposable
    {
        public ValueTask DisposeAsync() => ValueTask.CompletedTask;
    }
}
