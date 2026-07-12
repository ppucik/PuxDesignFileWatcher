using PuxDesignFileWatcher.Application.Abstractions.CQRS;
using PuxDesignFileWatcher.Domain.Entities;

namespace PuxDesignFileWatcher.Application.UseCases.SaveState;

/// <summary>
/// Command that persists the latest snapshot state.
/// </summary>
/// <param name="RootPath">Root directory path.</param>
/// <param name="Snapshot">Snapshot to persist.</param>
public sealed record SaveStateCommand(string RootPath, DirectoryManifestSnapshot Snapshot) : ICommand<bool>;
