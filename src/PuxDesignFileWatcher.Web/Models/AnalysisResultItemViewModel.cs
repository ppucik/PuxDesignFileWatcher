namespace PuxDesignFileWatcher.Web.Models;

/// <summary>
/// One change row for UI rendering.
/// </summary>
/// <param name="Path">Relative file path.</param>
/// <param name="Version">Computed file version.</param>
public sealed record AnalysisResultItemViewModel(string Path, int Version);
