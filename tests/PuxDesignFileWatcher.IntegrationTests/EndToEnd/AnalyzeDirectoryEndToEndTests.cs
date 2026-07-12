using Microsoft.Extensions.Logging.Abstractions;
using PuxDesignFileWatcher.Application.Abstractions.CQRS;
using PuxDesignFileWatcher.Application.Models;
using PuxDesignFileWatcher.Application.Ports;
using PuxDesignFileWatcher.Application.UseCases.AnalyzeDirectory;
using PuxDesignFileWatcher.Application.UseCases.DiffState;
using PuxDesignFileWatcher.Application.UseCases.LoadState;
using PuxDesignFileWatcher.Application.UseCases.SaveState;
using PuxDesignFileWatcher.Domain.Changes;
using PuxDesignFileWatcher.Domain.Entities;
using PuxDesignFileWatcher.Infrastructure.Analysis;
using PuxDesignFileWatcher.Infrastructure.Concurrency;
using PuxDesignFileWatcher.Infrastructure.FileSystem;
using PuxDesignFileWatcher.Infrastructure.Hashing;
using PuxDesignFileWatcher.Infrastructure.Storage;

namespace PuxDesignFileWatcher.IntegrationTests.EndToEnd;

public sealed class AnalyzeDirectoryEndToEndTests : IAsyncLifetime
{
    private string _tempRoot = string.Empty;
    private string _manifestPath = string.Empty;

    [Fact]
    public async Task AnalyzeDirectory_AcrossTwoRuns_ShouldReportNewThenChanged()
    {
        var analyzer = CreateAnalyzer();

        var filePath = Path.Combine(_tempRoot, "sample.txt");
        await File.WriteAllTextAsync(filePath, "version-1", CancellationToken.None);
        File.SetLastWriteTimeUtc(filePath, DateTime.Parse("2026-01-01T00:00:00Z").ToUniversalTime());

        var first = await analyzer.HandleAsync(new AnalyzeDirectoryCommand(_tempRoot), CancellationToken.None);

        Assert.Contains(first.Changes, c => c.RelativePath == "sample.txt" && c.ChangeType == ChangeType.New && c.Version == 1);

        await File.WriteAllTextAsync(filePath, "version-2", CancellationToken.None);
        File.SetLastWriteTimeUtc(filePath, DateTime.Parse("2026-01-02T00:00:00Z").ToUniversalTime());

        var second = await analyzer.HandleAsync(new AnalyzeDirectoryCommand(_tempRoot), CancellationToken.None);

        Assert.Contains(second.Changes, c => c.RelativePath == "sample.txt" && c.ChangeType == ChangeType.Changed && c.Version == 2);
    }

    [Fact]
    public async Task AnalyzeDirectory_WhenSubdirectoryIsDeleted_ShouldReportDeletedFiles()
    {
        var analyzer = CreateAnalyzer();

        var nestedDirectory = Path.Combine(_tempRoot, "nested");
        Directory.CreateDirectory(nestedDirectory);

        var nestedFile = Path.Combine(nestedDirectory, "child.txt");
        await File.WriteAllTextAsync(nestedFile, "data", CancellationToken.None);
        File.SetLastWriteTimeUtc(nestedFile, DateTime.Parse("2026-01-03T00:00:00Z").ToUniversalTime());

        _ = await analyzer.HandleAsync(new AnalyzeDirectoryCommand(_tempRoot), CancellationToken.None);

        Directory.Delete(nestedDirectory, recursive: true);

        var second = await analyzer.HandleAsync(new AnalyzeDirectoryCommand(_tempRoot), CancellationToken.None);

        Assert.Contains(second.Changes, c =>
            c.RelativePath == Path.Combine("nested", "child.txt")
            && c.ChangeType == ChangeType.Deleted);
    }

    [Fact]
    public async Task AnalyzeDirectory_WhenEmptySubdirectoryIsDeleted_ShouldReportDeletedDirectory()
    {
        var analyzer = CreateAnalyzer();

        var emptyDirectory = Path.Combine(_tempRoot, "empty-subdir");
        Directory.CreateDirectory(emptyDirectory);

        _ = await analyzer.HandleAsync(new AnalyzeDirectoryCommand(_tempRoot), CancellationToken.None);

        Directory.Delete(emptyDirectory, recursive: true);

        var second = await analyzer.HandleAsync(new AnalyzeDirectoryCommand(_tempRoot), CancellationToken.None);

        Assert.Contains(second.Changes, c =>
            c.RelativePath == "empty-subdir"
            && c.ChangeType == ChangeType.Deleted
            && c.EntryKind == ChangeEntryKind.Directory);
    }

    public Task InitializeAsync()
    {
        _tempRoot = Path.Combine(Path.GetTempPath(), "PuxDesignFileWatcherIntegration", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_tempRoot);
        _manifestPath = Path.Combine(_tempRoot, ".manifest-test.json");
        return Task.CompletedTask;
    }

    public Task DisposeAsync()
    {
        if (Directory.Exists(_tempRoot))
        {
            Directory.Delete(_tempRoot, recursive: true);
        }

        return Task.CompletedTask;
    }

    private ICommandHandler<AnalyzeDirectoryCommand, DirectoryAnalysisResult> CreateAnalyzer()
    {
        IAnalysisLockPort analysisLock = new KeyedAnalysisLockService();
        IAnalysisLimitsPort analysisLimits = new FixedAnalysisLimitsPort(maxFileCount: 100, maxFileSizeBytes: 50 * 1024L * 1024L);
        IFileSystemTraversalPort traversal = new FileSystemTraversalService(NullLogger<FileSystemTraversalService>.Instance);
        IContentHashPort hashing = new Sha256ContentHashProvider();
        IHybridFileStateBuilderPort hybridBuilder = new HybridFileStateBuilder(hashing, NullLogger<HybridFileStateBuilder>.Instance);

        IManifestPathResolverPort pathResolver = new FixedManifestPathResolver(_manifestPath);
        IManifestRepositoryPort repository = new JsonManifestRepository();

        var loadState = new LoadStateQueryHandler(pathResolver, repository);
        var diffState = new DiffStateCommandHandler();
        var saveState = new SaveStateCommandHandler(pathResolver, repository);

        return new AnalyzeDirectoryCommandHandler(analysisLock, analysisLimits, traversal, hybridBuilder, loadState, diffState, saveState);
    }

    private sealed class FixedManifestPathResolver : IManifestPathResolverPort
    {
        private readonly string _manifestPath;

        public FixedManifestPathResolver(string manifestPath)
        {
            _manifestPath = manifestPath;
        }

        public string ResolveManifestPath(string rootPath) => _manifestPath;
    }

    private sealed class FixedAnalysisLimitsPort : IAnalysisLimitsPort
    {
        private readonly AnalysisLimitsSettings _limits;

        public FixedAnalysisLimitsPort(int maxFileCount, long maxFileSizeBytes)
        {
            _limits = new AnalysisLimitsSettings(maxFileCount, maxFileSizeBytes);
        }

        public AnalysisLimitsSettings GetLimits() => _limits;
    }
}
