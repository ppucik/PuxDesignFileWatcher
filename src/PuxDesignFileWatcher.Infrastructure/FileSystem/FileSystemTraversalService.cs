using Microsoft.Extensions.Logging;
using PuxDesignFileWatcher.Application.Models;
using PuxDesignFileWatcher.Application.Ports;

namespace PuxDesignFileWatcher.Infrastructure.FileSystem;

/// <summary>
/// Recursively enumerates files from the local filesystem.
/// </summary>
public sealed class FileSystemTraversalService : IFileSystemTraversalPort
{
    private readonly ILogger<FileSystemTraversalService> _logger;

    public FileSystemTraversalService(ILogger<FileSystemTraversalService> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc />
    public Task<IReadOnlyCollection<ScannedFileDescriptor>> EnumerateFilesAsync(
        string rootPath,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(rootPath))
        {
            throw new ArgumentException("Root path must be provided.", nameof(rootPath));
        }

        var normalizedRoot = Path.GetFullPath(rootPath);
        var result = new List<ScannedFileDescriptor>();
        var pendingDirectories = new Stack<string>();
        pendingDirectories.Push(normalizedRoot);

        while (pendingDirectories.Count > 0)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var currentDirectory = pendingDirectories.Pop();

            IEnumerable<string> subDirectories;
            try
            {
                subDirectories = Directory.EnumerateDirectories(currentDirectory);
            }
            catch (IOException ex)
            {
                _logger.LogWarning(ex, "Skipping locked directory during traversal: {Directory}", currentDirectory);
                continue;
            }

            foreach (var subDirectory in subDirectories)
            {
                pendingDirectories.Push(subDirectory);
            }

            IEnumerable<string> files;
            try
            {
                files = Directory.EnumerateFiles(currentDirectory);
            }
            catch (IOException ex)
            {
                _logger.LogWarning(ex, "Skipping files in locked directory: {Directory}", currentDirectory);
                continue;
            }

            foreach (var filePath in files)
            {
                cancellationToken.ThrowIfCancellationRequested();

                try
                {
                    var fileInfo = new FileInfo(filePath);
                    var relativePath = Path.GetRelativePath(normalizedRoot, fileInfo.FullName);

                    result.Add(new ScannedFileDescriptor(
                        fileInfo.FullName,
                        relativePath,
                        fileInfo.Length,
                        fileInfo.LastWriteTimeUtc));
                }
                catch (IOException ex)
                {
                    _logger.LogWarning(ex, "Skipping locked file during traversal: {FilePath}", filePath);
                }
            }
        }

        return Task.FromResult<IReadOnlyCollection<ScannedFileDescriptor>>(result);
    }
}
