namespace PuxDesignFileWatcher.Application.Abstractions.Cqrs;

/// <summary>
/// Represents a command in CQRS style.
/// </summary>
public interface ICommand<out TResult>
{
}
