using PuxDesignFileWatcher.Domain.Changes;

namespace PuxDesignFileWatcher.Web.Models;

/// <summary>
/// One change row for UI rendering.
/// </summary>
/// <param name="Path">Relative file path.</param>
/// <param name="Version">Computed file version.</param>
/// <param name="EntryKind">Kind of changed entry (file or directory).</param>
public sealed record AnalysisResultItemViewModel(string Path, int Version, ChangeEntryKind EntryKind);
