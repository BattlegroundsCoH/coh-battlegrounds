using System;
using System.Windows.Input;

namespace Battlegrounds.UI;

/// <summary>
/// A command implementation
/// </summary>
/// <typeparam name="T"></typeparam>
public class RelayCommand<T> : ICommand {

    private readonly Action<T?> _execute;
    private readonly Func<T?, bool> _canExecute;

    public event EventHandler? CanExecuteChanged {
        add { CommandManager.RequerySuggested += value; }
        remove { CommandManager.RequerySuggested -= value; }
    }

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
/// 
/// </summary>
public class RelayCommand : RelayCommand<object> {

    public RelayCommand(Action execute) : base(_ => execute()) { }

    public RelayCommand(Action execute, Func<bool> canExecute) : base(_ => execute(), _ => canExecute()) { }

}
