namespace PuxDesignFileWatcher.Infrastructure.Configuration;

/// <summary>
/// Supported manifest location modes.
/// </summary>
public enum ManifestLocationMode
{
    /// <summary>
    /// Store manifests under application data base path.
    /// </summary>
    AppData = 1,

    /// <summary>
    /// Store manifests under analyzed root directory.
    /// </summary>
    PerRoot = 2
}
