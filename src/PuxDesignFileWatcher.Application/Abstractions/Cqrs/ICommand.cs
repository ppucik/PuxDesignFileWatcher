namespace PuxDesignFileWatcher.Application.Abstractions.CQRS;

/// <summary>
/// Represents a command in CQRS style.
/// </summary>
public interface ICommand<out TResult>
{
}
