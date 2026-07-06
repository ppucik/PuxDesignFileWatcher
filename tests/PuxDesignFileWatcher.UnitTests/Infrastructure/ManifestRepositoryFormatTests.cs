using PuxDesignFileWatcher.Domain.Entities;
using PuxDesignFileWatcher.Infrastructure.Storage;

namespace PuxDesignFileWatcher.UnitTests.Infrastructure;

public sealed class ManifestRepositoryFormatTests : IAsyncLifetime
{
    private string _tempDirectory = string.Empty;

    [Fact]
    public async Task JsonManifestRepository_ShouldRoundTripSnapshot()
    {
        var path = Path.Combine(_tempDirectory, "state.json");
        var repository = new JsonManifestRepository();

        var expected = CreateSnapshot();
        await repository.SaveAsync(path, expected, CancellationToken.None);

        var loaded = await repository.LoadAsync(path, CancellationToken.None);

        Assert.NotNull(loaded);
        Assert.Equal(expected.RootPath, loaded!.RootPath);
        Assert.Equal(expected.Files.Keys.OrderBy(x => x), loaded.Files.Keys.OrderBy(x => x));
        Assert.Equal(expected.Files["a.txt"].ContentHash, loaded.Files["a.txt"].ContentHash);
    }

    [Fact]
    public async Task MessagePackManifestRepository_ShouldRoundTripSnapshot()
    {
        var path = Path.Combine(_tempDirectory, "state.msgpack");
        var repository = new MessagePackManifestRepository();

        var expected = CreateSnapshot();
        await repository.SaveAsync(path, expected, CancellationToken.None);

        var loaded = await repository.LoadAsync(path, CancellationToken.None);

        Assert.NotNull(loaded);
        Assert.Equal(expected.RootPath, loaded!.RootPath);
        Assert.Equal(expected.Files["b.txt"].Version, loaded.Files["b.txt"].Version);
        Assert.Equal(expected.Files["b.txt"].ContentHash, loaded.Files["b.txt"].ContentHash);
    }

    public Task InitializeAsync()
    {
        _tempDirectory = Path.Combine(Path.GetTempPath(), "PuxDesignFileWatcherTests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_tempDirectory);
        return Task.CompletedTask;
    }

    public Task DisposeAsync()
    {
        if (Directory.Exists(_tempDirectory))
        {
            Directory.Delete(_tempDirectory, recursive: true);
        }

        return Task.CompletedTask;
    }

    private static DirectoryManifestSnapshot CreateSnapshot()
    {
        var files = new Dictionary<string, FileManifestEntry>(StringComparer.OrdinalIgnoreCase)
        {
            ["a.txt"] = new("a.txt", 10, DateTimeOffset.Parse("2026-01-01T00:00:00Z"), "HASH-A", 1),
            ["b.txt"] = new("b.txt", 20, DateTimeOffset.Parse("2026-01-01T00:00:00Z"), "HASH-B", 2)
        };

        return new DirectoryManifestSnapshot("C:\\deterministic-root", DateTimeOffset.Parse("2026-01-02T00:00:00Z"), files);
    }
}
