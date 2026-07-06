namespace PuxDesignFileWatcher.Application.Abstractions.Cqrs;

/// <summary>
/// Handles query execution.
/// </summary>
public interface IQueryHandler<in TQuery, TResult>
    where TQuery : IQuery<TResult>
{
    /// <summary>
    /// Executes query logic.
    /// </summary>
    Task<TResult> HandleAsync(TQuery query, CancellationToken cancellationToken);
}
