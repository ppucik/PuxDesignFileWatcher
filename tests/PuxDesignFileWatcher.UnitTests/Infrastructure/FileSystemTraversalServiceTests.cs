using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using PuxDesignFileWatcher.Infrastructure.Configuration;
using PuxDesignFileWatcher.Infrastructure.FileSystem;

namespace PuxDesignFileWatcher.UnitTests.Infrastructure;

public sealed class FileSystemTraversalServiceTests : IAsyncLifetime
{
    private string _rootPath = string.Empty;

    [Fact]
    public async Task EnumerateFilesAsync_PerRootMode_ShouldIgnoreConfiguredBasePathDirectory()
    {
        var configData = new Dictionary<string, string?>
        {
            ["Mode"] = "PerRoot",
            ["BasePath"] = StorageOptions.BASEPATH_DEFAULT
        };

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configData)
            .Build();

        var storageOptions = new StorageOptionsAccessor(configuration);
        var traversal = new FileSystemTraversalService(NullLogger<FileSystemTraversalService>.Instance, storageOptions);

        var trackedFile = Path.Combine(_rootPath, "user.txt");
        var internalDir = Path.Combine(_rootPath, StorageOptions.BASEPATH_DEFAULT);
        var internalFile = Path.Combine(internalDir, "manifest.manifest.json");

        Directory.CreateDirectory(internalDir);
        await File.WriteAllTextAsync(trackedFile, "user-content", CancellationToken.None);
        await File.WriteAllTextAsync(internalFile, "internal-content", CancellationToken.None);

        var snapshot = await traversal.EnumerateFilesAsync(_rootPath, CancellationToken.None);

        Assert.Contains(snapshot.Files, file => file.RelativePath.Equals("user.txt", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(snapshot.Files, file => file.RelativePath.StartsWith(StorageOptions.BASEPATH_DEFAULT, StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task EnumerateFilesAsync_WhenRootDoesNotExist_ShouldThrowDirectoryNotFoundException()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>())
            .Build();

        var storageOptions = new StorageOptionsAccessor(configuration);
        var traversal = new FileSystemTraversalService(NullLogger<FileSystemTraversalService>.Instance, storageOptions);

        var missingRoot = Path.Combine(_rootPath, "missing-root");

        await Assert.ThrowsAsync<DirectoryNotFoundException>(() =>
            traversal.EnumerateFilesAsync(missingRoot, CancellationToken.None));
    }

    public Task InitializeAsync()
    {
        _rootPath = Path.Combine(Path.GetTempPath(), "PuxDesignFileWatcherTraversalTests", Guid.NewGuid().ToString("N"));
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
}
