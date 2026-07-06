namespace PuxDesignFileWatcher.Application.Ports;

/// <summary>
/// Abstraction for resolving manifest file path for a root directory.
/// </summary>
public interface IManifestPathResolverPort
{
    /// <summary>
    /// Resolves manifest path where snapshot is persisted.
    /// </summary>
    string ResolveManifestPath(string rootPath);
}
