namespace PuxDesignFileWatcher.Application.UseCases.AnalyzeDirectory;

/// <summary>
/// Represents a validation failure for requested directory analysis.
/// </summary>
public sealed class DirectoryAnalysisValidationException : Exception
{
    /// <summary>
    /// Initializes a new validation exception.
    /// </summary>
    public DirectoryAnalysisValidationException(string message)
        : base(message)
    {
    }
}
