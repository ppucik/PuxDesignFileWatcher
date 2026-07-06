using PuxDesignFileWatcher.Domain.Changes;
using PuxDesignFileWatcher.Domain.Entities;

namespace PuxDesignFileWatcher.Application.UseCases.AnalyzeDirectory;

/// <summary>
/// Result of a directory analysis run.
/// </summary>
/// <param name="RootPath">Analyzed root path.</param>
/// <param name="AnalyzedAtUtc">Analysis timestamp in UTC.</param>
/// <param name="Changes">Detected file changes.</param>
/// <param name="Snapshot">Snapshot persisted as the latest state.</param>
public sealed record DirectoryAnalysisResult(
    string RootPath,
    DateTimeOffset AnalyzedAtUtc,
    IReadOnlyCollection<FileChangeRecord> Changes,
    DirectoryManifestSnapshot Snapshot);
