namespace PuxDesignFileWatcher.Application.Models;

/// <summary>
/// Represents scanned directory content including files and subdirectories.
/// </summary>
/// <param name="Files">Scanned files.</param>
/// <param name="Directories">Scanned relative subdirectory paths.</param>
public sealed record ScannedDirectorySnapshot(
    IReadOnlyCollection<ScannedFileDescriptor> Files,
    IReadOnlyCollection<string> Directories);
