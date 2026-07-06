using Microsoft.Extensions.Logging.Abstractions;
using PuxDesignFileWatcher.Application.Models;
using PuxDesignFileWatcher.Application.Ports;
using PuxDesignFileWatcher.Domain.Entities;
using PuxDesignFileWatcher.Infrastructure.Analysis;

namespace PuxDesignFileWatcher.UnitTests.Infrastructure;

public class HybridFileStateBuilderTests
{
    [Fact]
    public async Task BuildCurrentStateAsync_ShouldReuseHash_WhenSizeAndTimestampAreUnchanged()
    {
        var hashPort = new RecordingHashPort("UNUSED");
        var builder = new HybridFileStateBuilder(hashPort, NullLogger<HybridFileStateBuilder>.Instance);

        var timestamp = DateTimeOffset.Parse("2026-01-01T10:00:00Z");
        var previous = new DirectoryManifestSnapshot(
            "C:\\root",
            timestamp,
            new Dictionary<string, FileManifestEntry>
            {
                ["file.txt"] = new("file.txt", 10, timestamp, "PREVIOUS-HASH", 7)
            });

        var scanned = new[]
        {
            new ScannedFileDescriptor("C:\\root\\file.txt", "file.txt", 10, timestamp)
        };

        var result = await builder.BuildCurrentStateAsync(scanned, previous, CancellationToken.None);

        Assert.Equal(0, hashPort.CallCount);
        Assert.Equal("PREVIOUS-HASH", result["file.txt"].ContentHash);
        Assert.Equal(7, result["file.txt"].Version);
    }

    [Fact]
    public async Task BuildCurrentStateAsync_ShouldComputeHash_WhenMetadataChanged()
    {
        var hashPort = new RecordingHashPort("COMPUTED-HASH");
        var builder = new HybridFileStateBuilder(hashPort, NullLogger<HybridFileStateBuilder>.Instance);

        var previous = new DirectoryManifestSnapshot(
            "C:\\root",
            DateTimeOffset.Parse("2026-01-01T10:00:00Z"),
            new Dictionary<string, FileManifestEntry>
            {
                ["file.txt"] = new("file.txt", 10, DateTimeOffset.Parse("2026-01-01T10:00:00Z"), "PREVIOUS-HASH", 2)
            });

        var scanned = new[]
        {
            new ScannedFileDescriptor("C:\\root\\file.txt", "file.txt", 11, DateTimeOffset.Parse("2026-01-01T10:00:00Z"))
        };

        var result = await builder.BuildCurrentStateAsync(scanned, previous, CancellationToken.None);

        Assert.Equal(1, hashPort.CallCount);
        Assert.Equal("COMPUTED-HASH", result["file.txt"].ContentHash);
        Assert.Equal(2, result["file.txt"].Version);
    }

    private sealed class RecordingHashPort : IContentHashPort
    {
        private readonly string _hashToReturn;

        public RecordingHashPort(string hashToReturn)
        {
            _hashToReturn = hashToReturn;
        }

        public int CallCount { get; private set; }

        public Task<string> ComputeHashAsync(string filePath, CancellationToken cancellationToken)
        {
            CallCount++;
            return Task.FromResult(_hashToReturn);
        }
    }
}
