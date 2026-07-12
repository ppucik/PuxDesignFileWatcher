using PuxDesignFileWatcher.Domain.Changes;
using PuxDesignFileWatcher.Domain.Entities;
using PuxDesignFileWatcher.Domain.Services;

namespace PuxDesignFileWatcher.UnitTests.Domain;

public class ManifestDiffServiceTests
{
    [Fact]
    public void Diff_ShouldClassifyNewChangedAndDeletedFiles()
    {
        var previousSnapshot = new DirectoryManifestSnapshot(
            "C:\\root",
            DateTimeOffset.Parse("2026-01-01T00:00:00Z"),
            new Dictionary<string, FileManifestEntry>(StringComparer.OrdinalIgnoreCase)
            {
                ["existing.txt"] = new("existing.txt", 100, DateTimeOffset.Parse("2026-01-01T00:00:00Z"), "HASH-A", 1),
                ["deleted.txt"] = new("deleted.txt", 50, DateTimeOffset.Parse("2026-01-01T00:00:00Z"), "HASH-DEL", 3)
            });

        var currentFiles = new Dictionary<string, FileManifestEntry>(StringComparer.OrdinalIgnoreCase)
        {
            ["existing.txt"] = new("existing.txt", 100, DateTimeOffset.Parse("2026-01-02T00:00:00Z"), "HASH-B", 1),
            ["new.txt"] = new("new.txt", 70, DateTimeOffset.Parse("2026-01-02T00:00:00Z"), "HASH-NEW", 1)
        };

        var result = ManifestDiffService.Diff(
            previousSnapshot,
            "C:\\root",
            currentFiles,
            [],
            DateTimeOffset.Parse("2026-01-02T00:00:00Z"));

        Assert.Contains(result.Changes, c => c.RelativePath == "new.txt" && c.ChangeType == ChangeType.New && c.Version == 1);
        Assert.Contains(result.Changes, c => c.RelativePath == "existing.txt" && c.ChangeType == ChangeType.Changed && c.Version == 2);
        Assert.Contains(result.Changes, c => c.RelativePath == "deleted.txt" && c.ChangeType == ChangeType.Deleted && c.Version == 3);
    }

    [Fact]
    public void Diff_ShouldIncrementVersionOnlyWhenContentChanged()
    {
        var previousSnapshot = new DirectoryManifestSnapshot(
            "C:\\root",
            DateTimeOffset.Parse("2026-01-01T00:00:00Z"),
            new Dictionary<string, FileManifestEntry>(StringComparer.OrdinalIgnoreCase)
            {
                ["stable.txt"] = new("stable.txt", 100, DateTimeOffset.Parse("2026-01-01T00:00:00Z"), "SAME", 4),
                ["changed.txt"] = new("changed.txt", 100, DateTimeOffset.Parse("2026-01-01T00:00:00Z"), "OLD", 2)
            });

        var currentFiles = new Dictionary<string, FileManifestEntry>(StringComparer.OrdinalIgnoreCase)
        {
            ["stable.txt"] = new("stable.txt", 200, DateTimeOffset.Parse("2026-01-02T00:00:00Z"), "SAME", 1),
            ["changed.txt"] = new("changed.txt", 100, DateTimeOffset.Parse("2026-01-02T00:00:00Z"), "NEW", 1)
        };

        var result = ManifestDiffService.Diff(
            previousSnapshot,
            "C:\\root",
            currentFiles,
            [],
            DateTimeOffset.Parse("2026-01-02T00:00:00Z"));

        var stable = result.NextSnapshot.Files["stable.txt"];
        var changed = result.NextSnapshot.Files["changed.txt"];

        Assert.Equal(4, stable.Version);
        Assert.Equal(3, changed.Version);
    }

    [Fact]
    public void Diff_ShouldReportDeletedEmptySubdirectory()
    {
        var previousSnapshot = new DirectoryManifestSnapshot(
            "C:\\root",
            DateTimeOffset.Parse("2026-01-01T00:00:00Z"),
            new Dictionary<string, FileManifestEntry>(StringComparer.OrdinalIgnoreCase),
            ["empty-subdir"]);

        var result = ManifestDiffService.Diff(
            previousSnapshot,
            "C:\\root",
            new Dictionary<string, FileManifestEntry>(StringComparer.OrdinalIgnoreCase),
            [],
            DateTimeOffset.Parse("2026-01-02T00:00:00Z"));

        Assert.Contains(result.Changes, change =>
            change.RelativePath == "empty-subdir"
            && change.ChangeType == ChangeType.Deleted
            && change.EntryKind == ChangeEntryKind.Directory);
    }
}
