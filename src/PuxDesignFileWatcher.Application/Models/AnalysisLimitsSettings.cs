namespace PuxDesignFileWatcher.Application.Models;

/// <summary>
/// Configured guard-rail limits for a single analysis run.
/// </summary>
/// <param name="MaxFileCount">Maximum number of files allowed for one analysis run.</param>
/// <param name="MaxFileSizeBytes">Maximum size of a single file in bytes.</param>
public sealed record AnalysisLimitsSettings(int MaxFileCount, long MaxFileSizeBytes);
