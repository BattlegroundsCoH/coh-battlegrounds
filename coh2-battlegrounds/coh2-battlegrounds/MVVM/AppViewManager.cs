using System;
using System.Collections.Generic;
using System.Windows;

using BattlegroundsApp.Modals;

using ViewModelType = Battlegrounds.Functional.Either<string, System.Type, BattlegroundsApp.MVVM.IViewModel?>;

namespace BattlegroundsApp.MVVM {

    public enum AppDisplayState {
        LeftRight,
        Full
    }

    public enum AppDisplayTarget {
        Left,
        Right,
        Full
    }

    public class AppViewManager {

        private HashSet<IViewModel> m_alive;
        private readonly Dictionary<string, IViewModel?> m_models;
        private readonly MainWindow m_window;

        public AppViewManager(MainWindow window) {
            this.m_models = new();
            this.m_window = window;
            this.m_alive = new();
        }

        public void SetDisplay(AppDisplayState displayState, ViewModelType left, ViewModelType right) {
            if (displayState == AppDisplayState.LeftRight) {
                this.UpdateDisplayInternal(AppDisplayTarget.Left, this.SolveDisplay(left));
                this.UpdateDisplayInternal(AppDisplayTarget.Right, this.SolveDisplay(right));
            } else if (displayState == AppDisplayState.Full) {
                this.UpdateDisplayInternal(AppDisplayTarget.Full, this.SolveDisplay(left) ?? this.SolveDisplay(right)); // Use left, and right if left was invalid.
            } else {
                throw new NotImplementedException();
            }
        }

        public IViewModel? SolveDisplay(ViewModelType view) {
            if (view.IfFirstOption(out string? str)) {
                return this.m_models?.GetValueOrDefault(str, null);
            } else if (view.IfSecondOption(out var t)) {
                return this.m_models.GetValueOrDefault(t.Name, null);
            } else if (view.IfThirdOption(out var mdl)) {
                string tyKey = mdl.GetType().Name;
                if (mdl.SingleInstanceOnly) {
                    if (this.m_models.TryGetValue(tyKey, out var model)) {
                        return model;
                    }
                    return this.m_models[tyKey] = mdl;
                } else {
                    if (this.m_models.TryGetValue(tyKey, out var model) && model is not null && !model.UnloadViewModel()) {
                        return model;
                    }
                    return this.m_models[tyKey] = mdl;
                }
            }
            return null;
        }

        public void UpdateDisplay(AppDisplayTarget target, ViewModelType display) {
            if (this.SolveDisplay(display) is IViewModel model && this.UpdateDisplayInternal(target, model)) {
                return;
            }
            throw new InvalidOperationException();
        }

        public T UpdateDisplay<T>(AppDisplayTarget target) where T : IViewModel {
            if (this.SolveDisplay(typeof(T)) is T model) {
                return this.UpdateDisplayInternal(target, model) ? model : throw new InvalidOperationException();
            } else {
                throw new InvalidOperationException();
            }
        }

        private bool UpdateDisplayInternal(AppDisplayTarget target, IViewModel model) {
            
            // Ensure valid operation
            if (target == AppDisplayTarget.Full && this.m_window.DisplayState != AppDisplayState.Full) {
                throw new InvalidOperationException();
            }
            
            // If single instance, notify of swapback
            if (model.KeepAlive) {
                if (!this.m_alive.Add(model)) {
                    Application.Current.Dispatcher.Invoke(() => {
                        model.Swapback();
                    });
                }
            }

            // Switch on where to set
            switch (target) {
                case AppDisplayTarget.Left:
                    this.m_window.SetLeftPanel(model);
                    break;
                case AppDisplayTarget.Right:
                    this.m_window.SetRightPanel(model);
                    break;
                case AppDisplayTarget.Full:
                    this.m_window.SetFull(model);
                    break;
                default:
                    return false;
            }
            
            // Return true -> is set
            return true;

        }

        public T? CreateDisplayIfNotFound<T>(Func<T> creator) where T : class, IViewModel
            => (this.m_models.TryGetValue(typeof(T).Name, out var model) ? model : (this.m_models[typeof(T).Name] = creator())) as T;

        public bool CloseDisplay<T>()
            => this.m_models.TryGetValue(typeof(T).Name, out var model) && model is not null && model.UnloadViewModel();

        public ModalControl? GetModalControl() 
            => this.m_window.ModalView;

        public ModalControl? GetRightsideModalControl()
            => this.m_window.RightModalView;

    }

}
