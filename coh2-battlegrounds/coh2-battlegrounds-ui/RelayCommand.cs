using System;
using System.Windows.Input;

namespace Battlegrounds.UI;

/// <summary>
/// Class implementing the <see cref="ICommand"/> interface for UI commands.
/// </summary>
/// <typeparam name="T">Command argument type</typeparam>
public class RelayCommand<T> : ICommand {

    private readonly Action<T?> _execute;
    private readonly Func<T?, bool> _canExecute;

    public event EventHandler? CanExecuteChanged {
        add { CommandManager.RequerySuggested += value; }
        remove { CommandManager.RequerySuggested -= value; }
    }

    /// <summary>
    /// Initialise a new <see cref="RelayCommand{T}"/> instance.
    /// </summary>
    /// <param name="execute">The action to execute.</param>
    /// <param name="canExecute">The test function to determine if the command can be executed.</param>
    /// <exception cref="ArgumentNullException"></exception>
    public RelayCommand(Action<T?> execute, Func<T?, bool>? canExecute = null) {

        this._execute = execute ?? throw new ArgumentNullException(nameof(execute));
        this._canExecute = canExecute ?? (_ => true);

    }

    public bool CanExecute(object? parameter) => this._canExecute(parameter is T t ? t : default);

    public virtual void Execute(object? parameter) {
        if (parameter is T t) {
            this._execute(t);
        } else {
            T? tres = (T?)Convert.ChangeType(parameter, typeof(T));
            this._execute(tres);
        }
    }

}

/// <summary>
/// Class extension of <see cref="RelayCommand{T}"/> with type argument set to <see langword="object"/>.
/// </summary>
public sealed class RelayCommand : RelayCommand<object> {

    /// <summary>
    /// Initialise a new <see cref="RelayCommand"/> instance with an action to execute.
    /// </summary>
    /// <param name="execute">The action to execute.</param>
    public RelayCommand(Action execute) : base(_ => execute()) { }

    /// <summary>
    /// Initialise a new <see cref="RelayCommand"/> instance with an action to execute and a function to determine if command can be executed.
    /// </summary>
    /// <param name="execute">The action to execute.</param>
    /// <param name="canExecute">The function to use to determine if command can be executed.</param>
    public RelayCommand(Action execute, Func<bool> canExecute) : base(_ => execute(), _ => canExecute()) { }

}
