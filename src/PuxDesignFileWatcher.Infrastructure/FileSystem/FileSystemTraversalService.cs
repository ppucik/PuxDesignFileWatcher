using Microsoft.Extensions.Logging;
using PuxDesignFileWatcher.Application.Models;
using PuxDesignFileWatcher.Application.Ports;
using PuxDesignFileWatcher.Infrastructure.Configuration;

namespace PuxDesignFileWatcher.Infrastructure.FileSystem;

/// <summary>
/// Recursively enumerates files from the local filesystem.
/// </summary>
public sealed class FileSystemTraversalService : IFileSystemTraversalPort
{
    private readonly ILogger<FileSystemTraversalService> _logger;
    private readonly StorageOptionsAccessor? _storageOptions;

    public FileSystemTraversalService(ILogger<FileSystemTraversalService> logger)
        : this(logger, storageOptions: null)
    {
    }

    public FileSystemTraversalService(
        ILogger<FileSystemTraversalService> logger,
        StorageOptionsAccessor? storageOptions)
    {
        _logger = logger;
        _storageOptions = storageOptions;
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

        if (!Directory.Exists(normalizedRoot))
        {
            throw new DirectoryNotFoundException($"Directory '{normalizedRoot}' does not exist.");
        }

        var ignoredDirectory = ResolveIgnoredDirectoryForPerRootMode(normalizedRoot);

        var result = new List<ScannedFileDescriptor>();
        var pendingDirectories = new Stack<string>();
        pendingDirectories.Push(normalizedRoot);

        while (pendingDirectories.Count > 0)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var currentDirectory = pendingDirectories.Pop();

            if (ignoredDirectory is not null && IsSameOrSubPath(currentDirectory, ignoredDirectory))
            {
                continue;
            }

            IEnumerable<string> subDirectories;
            try
            {
                subDirectories = Directory.EnumerateDirectories(currentDirectory);
            }
            catch (IOException ex)
            {
                if (ex is DirectoryNotFoundException && currentDirectory.Equals(normalizedRoot, StringComparison.OrdinalIgnoreCase))
                {
                    throw new DirectoryNotFoundException($"Directory '{normalizedRoot}' does not exist.", ex);
                }

                _logger.LogWarning(ex, "Skipping inaccessible directory during traversal: {Directory}", currentDirectory);
                continue;
            }

            foreach (var subDirectory in subDirectories)
            {
                if (ignoredDirectory is not null && IsSameOrSubPath(subDirectory, ignoredDirectory))
                {
                    continue;
                }

                pendingDirectories.Push(subDirectory);
            }

            IEnumerable<string> files;
            try
            {
                files = Directory.EnumerateFiles(currentDirectory);
            }
            catch (IOException ex)
            {
                if (ex is DirectoryNotFoundException && currentDirectory.Equals(normalizedRoot, StringComparison.OrdinalIgnoreCase))
                {
                    throw new DirectoryNotFoundException($"Directory '{normalizedRoot}' does not exist.", ex);
                }

                _logger.LogWarning(ex, "Skipping files in inaccessible directory: {Directory}", currentDirectory);
                continue;
            }

            foreach (var filePath in files)
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (ignoredDirectory is not null && IsSameOrSubPath(filePath, ignoredDirectory))
                {
                    continue;
                }

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

    private string? ResolveIgnoredDirectoryForPerRootMode(string normalizedRoot)
    {
        if (_storageOptions?.Mode != ManifestLocationMode.PerRoot)
        {
            return null;
        }

        if (string.IsNullOrWhiteSpace(_storageOptions.BasePath))
        {
            return null;
        }

        return Path.GetFullPath(Path.Combine(normalizedRoot, _storageOptions.BasePath));
    }

    private static bool IsSameOrSubPath(string path, string parentPath)
    {
        var normalizedPath = Path.GetFullPath(path)
            .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

        var normalizedParent = Path.GetFullPath(parentPath)
            .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

        return normalizedPath.Equals(normalizedParent, StringComparison.OrdinalIgnoreCase)
               || normalizedPath.StartsWith(normalizedParent + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase)
               || normalizedPath.StartsWith(normalizedParent + Path.AltDirectorySeparatorChar, StringComparison.OrdinalIgnoreCase);
    }
}
