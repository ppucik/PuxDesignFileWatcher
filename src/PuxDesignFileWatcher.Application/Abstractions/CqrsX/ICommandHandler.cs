namespace PuxDesignFileWatcher.Application.Abstractions.CQRS;

/// <summary>
/// Handles command execution
/// </summary>
public interface ICommandHandler<in TCommand, TResult>
    where TCommand : ICommand<TResult>
{
    /// <summary>
    /// Executes command logic.
    /// </summary>
    Task<TResult> HandleAsync(TCommand command, CancellationToken cancellationToken);
}
