using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;

namespace BattlegroundsApp.Utilities {

    public class RelayCommand : ICommand {

        readonly Action<object> _execute;
        readonly Predicate<object> _canExecute;

        public RelayCommand(Action<object> execute, Predicate<object> canExecute) {

            if (execute == null) throw new NullReferenceException("execute");

            _execute = execute;
            _canExecute = canExecute;

        }

        public RelayCommand(Action<object> execute) : this(execute, null) {

        }

        public event EventHandler CanExecuteChanged {

            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }

        }

        public bool CanExecute(object parameter) {

            return _canExecute == null ? true : _canExecute(parameter);

        }

        public void Execute(object parameter) {

            _execute.Invoke(parameter);

        }

    }

}
