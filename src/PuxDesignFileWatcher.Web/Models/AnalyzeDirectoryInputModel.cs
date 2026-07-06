using System.ComponentModel.DataAnnotations;

namespace PuxDesignFileWatcher.Web.Models;

/// <summary>
/// Input model for manual directory analysis.
/// </summary>
public sealed class AnalyzeDirectoryInputModel
{
    /// <summary>
    /// Root directory path to analyze.
    /// </summary>
    [Required]
    [Display(Name = "Directory path")]
    public string RootPath { get; set; } = string.Empty;
}
