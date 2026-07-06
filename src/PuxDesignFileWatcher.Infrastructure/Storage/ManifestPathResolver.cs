using System.Security.Cryptography;
using System.Text;
using PuxDesignFileWatcher.Application.Ports;
using PuxDesignFileWatcher.Infrastructure.Configuration;

namespace PuxDesignFileWatcher.Infrastructure.Storage;

/// <summary>
/// Resolves manifest path from configured base path, mode, and format.
/// </summary>
public sealed class ManifestPathResolver : IManifestPathResolverPort
{
    private readonly StorageOptionsAccessor _storageOptions;

    public ManifestPathResolver(StorageOptionsAccessor storageOptions)
    {
        _storageOptions = storageOptions;
    }

    /// <inheritdoc />
    public string ResolveManifestPath(string rootPath)
    {
        if (string.IsNullOrWhiteSpace(rootPath))
        {
            throw new ArgumentException("Root path must be provided.", nameof(rootPath));
        }

        var extension = _storageOptions.StorageFormat == Configuration.StorageFormat.MessagePack
            ? ".manifest.msgpack"
            : ".manifest.json";

        var normalizedRoot = Path.GetFullPath(rootPath);
        var rootFingerprint = CreateRootFingerprint(normalizedRoot);

        if (_storageOptions.Mode == ManifestLocationMode.PerRoot)
        {
            var perRootBasePath = string.IsNullOrWhiteSpace(_storageOptions.BasePath)
                ? normalizedRoot
                : Path.Combine(normalizedRoot, _storageOptions.BasePath);

            Directory.CreateDirectory(perRootBasePath);
            return Path.Combine(perRootBasePath, $"{rootFingerprint}{extension}");
        }

        var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var configuredBasePath = string.IsNullOrWhiteSpace(_storageOptions.BasePath)
            ? "PuxDesignFileWatcher"
            : _storageOptions.BasePath;

        var storageBase = Path.IsPathRooted(configuredBasePath)
            ? configuredBasePath
            : Path.Combine(appDataPath, configuredBasePath);

        var storageDirectory = Path.Combine(storageBase, rootFingerprint);
        Directory.CreateDirectory(storageDirectory);

        return Path.Combine(storageDirectory, $"manifest{extension}");
    }

    private static string CreateRootFingerprint(string rootPath)
    {
        var safeName = new string(rootPath
            .Select(ch => Path.GetInvalidFileNameChars().Contains(ch) ? '_' : ch)
            .ToArray());

        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(rootPath));
        return $"{safeName}_{Convert.ToHexString(hash).Substring(0, 12)}";
    }
}
