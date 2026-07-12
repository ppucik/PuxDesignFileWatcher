namespace PuxDesignFileWatcher.Infrastructure.Configuration;

/// <summary>
/// Configurable analysis guard-rail limits.
/// </summary>
public sealed class AnalysisLimitsOptions
{
    /// <summary>
    /// Root section name.
    /// </summary>
    public const string SECTION_NAME = "AnalysisLimits";

    /// <summary>
    /// Default maximum number of files.
    /// </summary>
    public const int MaxFileCountDefault = 100;

    /// <summary>
    /// Default maximum single-file size in megabytes.
    /// </summary>
    public const int MaxFileSizeMbDefault = 50;

    /// <summary>
    /// Maximum number of files allowed for one analysis run.
    /// </summary>
    public int MaxFileCount { get; set; } = MaxFileCountDefault;

    /// <summary>
    /// Maximum size of one file in megabytes.
    /// </summary>
    public int MaxFileSizeMb { get; set; } = MaxFileSizeMbDefault;
}
