using System;
using System.Windows.Input;

namespace BattlegroundsApp.Utilities {

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class TargettedRelayCommand<TTarget, TArg> : ICommand {

        private readonly TTarget m_target;
        private readonly Action<TTarget, TArg> m_execute;
        private readonly Func<TTarget, TArg, bool> m_canExecute;

        public event EventHandler CanExecuteChanged {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public TargettedRelayCommand(TTarget target, Action<TTarget, TArg> execute) {
            this.m_canExecute = (_, _) => true;
            this.m_execute = execute;
            this.m_target = target;
        }

        public TargettedRelayCommand(TTarget target, Action<TTarget, TArg> execute, Func<TTarget, TArg, bool> canExecute) {
            this.m_canExecute = canExecute;
            this.m_execute = execute;
            this.m_target = target;
        }

        public bool CanExecute(object parameter) => this.m_canExecute?.Invoke(this.m_target, (TArg)parameter) ?? true;

        public void Execute(object parameter) => this.m_execute?.Invoke(this.m_target, (TArg)parameter);

    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class TargettedRelayCommand<TTarget> : ICommand {

        private readonly TTarget m_target;
        private readonly Action<TTarget> m_execute;
        private readonly Func<TTarget, bool> m_canExecute;

        public event EventHandler CanExecuteChanged {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public TargettedRelayCommand(TTarget target, Action<TTarget> execute) {
            this.m_canExecute = _ => true;
            this.m_execute = execute;
            this.m_target = target;
        }

        public TargettedRelayCommand(TTarget target, Action<TTarget> execute, Func<TTarget, bool> canExecute) {
            this.m_canExecute = canExecute;
            this.m_execute = execute;
            this.m_target = target;
        }

        public bool CanExecute(object parameter) => this.m_canExecute?.Invoke(this.m_target) ?? true;

        public void Execute(object parameter) => this.m_execute?.Invoke(this.m_target);

    }

}
