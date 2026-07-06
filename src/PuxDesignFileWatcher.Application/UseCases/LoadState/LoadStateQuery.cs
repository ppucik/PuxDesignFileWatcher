using PuxDesignFileWatcher.Application.Abstractions.Cqrs;
using PuxDesignFileWatcher.Domain.Entities;

namespace PuxDesignFileWatcher.Application.UseCases.LoadState;

/// <summary>
/// Query that loads previously persisted manifest state for a root path.
/// </summary>
/// <param name="RootPath">Root directory path.</param>
public sealed record LoadStateQuery(string RootPath) : IQuery<DirectoryManifestSnapshot?>;
