namespace PuxDesignFileWatcher.Domain.Entities;

/// <summary>
/// Represents a manifest snapshot of one analyzed root directory.
/// </summary>
public sealed record DirectoryManifestSnapshot
{
    /// <summary>
    /// Initializes a new manifest snapshot.
    /// </summary>
    /// <param name="rootPath">Analyzed root directory path.</param>
    /// <param name="analyzedAtUtc">Timestamp when analysis was completed.</param>
    /// <param name="files">Collection of file entries contained in the snapshot.</param>
    /// <param name="directories">Collection of relative subdirectory paths contained in the snapshot.</param>
    public DirectoryManifestSnapshot(
        string rootPath,
        DateTimeOffset analyzedAtUtc,
        IReadOnlyDictionary<string, FileManifestEntry> files,
        IReadOnlyCollection<string>? directories = null)
    {
        if (string.IsNullOrWhiteSpace(rootPath))
        {
            throw new ArgumentException("Root path must be provided.", nameof(rootPath));
        }

        ArgumentNullException.ThrowIfNull(files);

        RootPath = rootPath;
        AnalyzedAtUtc = analyzedAtUtc;
        Files = new Dictionary<string, FileManifestEntry>(files, StringComparer.OrdinalIgnoreCase);
        Directories = new HashSet<string>(
            directories ?? [],
            StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Gets the analyzed root directory path.
    /// </summary>
    public string RootPath { get; }

    /// <summary>
    /// Gets the UTC timestamp when this snapshot was analyzed.
    /// </summary>
    public DateTimeOffset AnalyzedAtUtc { get; }

    /// <summary>
    /// Gets file entries keyed by relative path.
    /// </summary>
    public IReadOnlyDictionary<string, FileManifestEntry> Files { get; }

    /// <summary>
    /// Gets relative subdirectory paths captured in this snapshot.
    /// </summary>
    public IReadOnlySet<string> Directories { get; }

    /// <summary>
    /// Creates an empty snapshot for a root path.
    /// </summary>
    /// <param name="rootPath">Analyzed root directory path.</param>
    /// <param name="analyzedAtUtc">Timestamp when analysis was completed.</param>
    /// <returns>An empty snapshot without file entries.</returns>
    public static DirectoryManifestSnapshot Empty(string rootPath, DateTimeOffset analyzedAtUtc)
        => new(
            rootPath,
            analyzedAtUtc,
            new Dictionary<string, FileManifestEntry>(StringComparer.OrdinalIgnoreCase),
            []);
}
