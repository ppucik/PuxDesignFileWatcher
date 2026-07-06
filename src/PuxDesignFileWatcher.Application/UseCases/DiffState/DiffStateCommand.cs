using PuxDesignFileWatcher.Application.Abstractions.Cqrs;
using PuxDesignFileWatcher.Domain.Changes;
using PuxDesignFileWatcher.Domain.Entities;

namespace PuxDesignFileWatcher.Application.UseCases.DiffState;

/// <summary>
/// Command that compares current scanned state with previously stored state.
/// </summary>
/// <param name="RootPath">Root directory path.</param>
/// <param name="PreviousSnapshot">Previously stored snapshot.</param>
/// <param name="CurrentScannedFiles">Current scanned files keyed by relative path.</param>
/// <param name="AnalyzedAtUtc">Current analysis timestamp in UTC.</param>
public sealed record DiffStateCommand(
    string RootPath,
    DirectoryManifestSnapshot? PreviousSnapshot,
    IReadOnlyDictionary<string, FileManifestEntry> CurrentScannedFiles,
    DateTimeOffset AnalyzedAtUtc) : ICommand<ManifestDiffResult>;
