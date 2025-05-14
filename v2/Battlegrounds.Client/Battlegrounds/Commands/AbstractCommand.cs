using System.Windows.Input;

namespace Battlegrounds.Commands;

public abstract class AbstractCommand<T> : ICommand {

    public event EventHandler? CanExecuteChanged;

    public bool CanExecute(object? parameter) {
        if (parameter is T t) {
            return CanExecuteCommand(t);
        } else if (parameter is null and default(object?)) {
            return CanExecuteCommand(default);
        } else {
            return false;
        }
    }

    public void Execute(object? parameter) {
        if (parameter is T t) {
            ExecuteCommand(t);
        } else if (parameter is null and default(object?)) { 
            ExecuteCommand(default);
        } else {
            throw new ArgumentException($"Parameter must be of type {typeof(T).Name}", nameof(parameter));
        }
    }

    protected virtual bool CanExecuteCommand(T? parameter) {
        return true;
    }

    protected abstract void ExecuteCommand(T? parameter);

}

public abstract class AbstractCommand : AbstractCommand<object?>;
