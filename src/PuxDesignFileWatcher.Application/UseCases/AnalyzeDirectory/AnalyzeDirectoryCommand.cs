using PuxDesignFileWatcher.Application.Abstractions.Cqrs;

namespace PuxDesignFileWatcher.Application.UseCases.AnalyzeDirectory;

/// <summary>
/// Command that triggers full directory analysis workflow.
/// </summary>
/// <param name="RootPath">Root directory path to analyze.</param>
public sealed record AnalyzeDirectoryCommand(string RootPath) : ICommand<DirectoryAnalysisResult>;
