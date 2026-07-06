namespace PuxDesignFileWatcher.Web.Models;

/// <summary>
/// View model for analysis page.
/// </summary>
public sealed class AnalyzeDirectoryPageViewModel
{
    /// <summary>
    /// User input model.
    /// </summary>
    public AnalyzeDirectoryInputModel Input { get; set; } = new();

    /// <summary>
    /// New files detected in current analysis.
    /// </summary>
    public IReadOnlyCollection<AnalysisResultItemViewModel> NewFiles { get; set; } = [];

    /// <summary>
    /// Changed files detected in current analysis.
    /// </summary>
    public IReadOnlyCollection<AnalysisResultItemViewModel> ChangedFiles { get; set; } = [];

    /// <summary>
    /// Deleted files detected in current analysis.
    /// </summary>
    public IReadOnlyCollection<AnalysisResultItemViewModel> DeletedFiles { get; set; } = [];

    /// <summary>
    /// Gets or sets analysis status message.
    /// </summary>
    public string? Message { get; set; }
}
