namespace PuxDesignFileWatcher.Domain.Entities;

/// <summary>
/// Represents one file entry persisted in a directory manifest snapshot.
/// </summary>
public sealed record FileManifestEntry
{
    /// <summary>
    /// Initializes a new file entry persisted in a snapshot.
    /// </summary>
    /// <param name="relativePath">Path relative to analyzed root directory.</param>
    /// <param name="sizeBytes">File size in bytes.</param>
    /// <param name="lastWriteTimeUtc">Last modification timestamp in UTC.</param>
    /// <param name="contentHash">Deterministic hash of file content.</param>
    /// <param name="version">Monotonic version of the file state starting at 1.</param>
    public FileManifestEntry(
        string relativePath,
        long sizeBytes,
        DateTimeOffset lastWriteTimeUtc,
        string contentHash,
        int version)
    {
        if (string.IsNullOrWhiteSpace(relativePath))
        {
            throw new ArgumentException("Relative path must be provided.", nameof(relativePath));
        }

        if (string.IsNullOrWhiteSpace(contentHash))
        {
            throw new ArgumentException("Content hash must be provided.", nameof(contentHash));
        }

        if (version < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(version), version, "File version must be greater than or equal to 1.");
        }

        RelativePath = relativePath;
        SizeBytes = sizeBytes;
        LastWriteTimeUtc = lastWriteTimeUtc;
        ContentHash = contentHash;
        Version = version;
    }

    /// <summary>
    /// Gets file path relative to analyzed root directory.
    /// </summary>
    public string RelativePath { get; }

    /// <summary>
    /// Gets file size in bytes.
    /// </summary>
    public long SizeBytes { get; }

    /// <summary>
    /// Gets last modification timestamp in UTC.
    /// </summary>
    public DateTimeOffset LastWriteTimeUtc { get; }

    /// <summary>
    /// Gets deterministic hash of file content.
    /// </summary>
    public string ContentHash { get; }

    /// <summary>
    /// Gets monotonic version of the file state.
    /// </summary>
    public int Version { get; }

    /// <summary>
    /// Creates a first-version entry for a newly discovered file.
    /// </summary>
    /// <param name="relativePath">Path relative to analyzed root directory.</param>
    /// <param name="sizeBytes">File size in bytes.</param>
    /// <param name="lastWriteTimeUtc">Last modification timestamp in UTC.</param>
    /// <param name="contentHash">Deterministic hash of file content.</param>
    /// <returns>Initialized file entry with version 1.</returns>
    public static FileManifestEntry CreateNew(
        string relativePath,
        long sizeBytes,
        DateTimeOffset lastWriteTimeUtc,
        string contentHash)
        => new(relativePath, sizeBytes, lastWriteTimeUtc, contentHash, 1);
}
