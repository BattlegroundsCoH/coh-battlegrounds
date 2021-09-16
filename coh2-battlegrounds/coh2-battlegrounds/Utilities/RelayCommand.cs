using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;

namespace BattlegroundsApp.Utilities {

    /// <summary>
    /// A command implementation
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class RelayCommand<T> : ICommand {

        private readonly Action<T> _execute;
        private readonly Func<T, bool> _canExecute;

        public event EventHandler CanExecuteChanged {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public RelayCommand(Action<T> execute, Func<T, bool> canExecute = null) {

            this._execute = execute ?? throw new ArgumentNullException(nameof(execute));
            this._canExecute = canExecute ?? (_ => true);

        }

        public bool CanExecute(object parameter) => this._canExecute((T)parameter);

        public virtual void Execute(object parameter) => this._execute((T)parameter);

    }

    /// <summary>
    /// 
    /// </summary>
    public class RelayCommand : RelayCommand<object> {

        public RelayCommand(Action execute) : base(_ => execute()) { }

        public RelayCommand(Action execute, Func<bool> canExecute) : base(_ => execute(), _ => canExecute()) { }

    }

}
