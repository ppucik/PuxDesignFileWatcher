namespace PuxDesignFileWatcher.Infrastructure.Configuration;

/// <summary>
/// Storage configuration loaded from appsettings.
/// </summary>
public sealed class StorageOptions
{
    /// <summary>
    /// Configuration section name.
    /// </summary>
    public const string SECTION_NAME = "Storage";

    /// <summary>
    /// Default base path for persisted manifests.
    /// </summary>
    public const string BASEPATH_DEFAULT = ".puxdfw";

    /// <summary>
    /// Gets or sets selected storage format.
    /// </summary>
    public string StorageFormat { get; set; } = nameof(Configuration.StorageFormat.Json);

    /// <summary>
    /// Gets or sets base path for manifest storage.
    /// </summary>
    public string BasePath { get; set; } = BASEPATH_DEFAULT;

    /// <summary>
    /// Gets or sets location mode: AppData or PerRoot.
    /// </summary>
    public string Mode { get; set; } = nameof(ManifestLocationMode.AppData);
}
