namespace PuxDesignFileWatcher.Application.Models;

/// <summary>
/// Represents one file discovered in the filesystem during analysis.
/// </summary>
/// <param name="FullPath">Absolute file path.</param>
/// <param name="RelativePath">Path relative to analyzed root.</param>
/// <param name="SizeBytes">File size in bytes.</param>
/// <param name="LastWriteTimeUtc">Last modification timestamp in UTC.</param>
public sealed record ScannedFileDescriptor(
    string FullPath,
    string RelativePath,
    long SizeBytes,
    DateTimeOffset LastWriteTimeUtc);
