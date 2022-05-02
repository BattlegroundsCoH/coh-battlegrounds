using System;
using System.Collections.Generic;
using System.Windows;

using BattlegroundsApp.Modals;

using ViewModelType = Battlegrounds.Functional.Either<string, System.Type, BattlegroundsApp.MVVM.IViewModel?>;

namespace BattlegroundsApp.MVVM;

public delegate void OnViewSwitched(IViewModel? model);

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

    private readonly HashSet<IViewModel> m_alive;
    private readonly Dictionary<string, IViewModel?> m_models;
    private readonly MainWindow m_window;

    public AppViewManager(MainWindow window) {
        this.m_models = new();
        this.m_window = window;
        this.m_alive = new();
    }

    public void SetDisplay(AppDisplayState displayState, ViewModelType left, ViewModelType right) {
        if (displayState == AppDisplayState.LeftRight) {
            this.UpdateDisplayInternal(AppDisplayTarget.Left, this.SolveDisplay(left), true, null);
            this.UpdateDisplayInternal(AppDisplayTarget.Right, this.SolveDisplay(right), true, null);
        } else if (displayState == AppDisplayState.Full) {
            this.UpdateDisplayInternal(AppDisplayTarget.Full, this.SolveDisplay(left) ?? this.SolveDisplay(right), true, null); // Use left, and right if left was invalid.
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
                if (this.m_models.TryGetValue(tyKey, out var model) && model is not null) {
                    return model;
                }
                return this.m_models[tyKey] = mdl;
            }
        }
        return null;
    }

    public void UpdateDisplay(AppDisplayTarget target, ViewModelType display, bool destroyView = true) {
        if (this.SolveDisplay(display) is IViewModel model) {
            this.UpdateDisplayInternal(target, model, destroyView, null);
        }
    }

    public void UpdateDisplay<T>(AppDisplayTarget target, bool destroyView = true) where T : IViewModel
        => this.UpdateDisplay<T>(target, null, destroyView);

    public void UpdateDisplay<T>(AppDisplayTarget target, OnViewSwitched? switchCallback, bool destroyView = true) where T : IViewModel {
        if (this.SolveDisplay(typeof(T)) is T model) {
            this.UpdateDisplayInternal(target, model, destroyView, switchCallback);
        } else {
            throw new InvalidOperationException();
        }
    }

    private void UpdateDisplayInternal(AppDisplayTarget target, IViewModel? model, bool destroyView, OnViewSwitched? switchCallback) {
        
        // Ensure valid operation
        if (target == AppDisplayTarget.Full && this.m_window.DisplayState != AppDisplayState.Full) {
            throw new InvalidOperationException();
        }

        // Get old content
        var old = target switch {
            AppDisplayTarget.Left or AppDisplayTarget.Full => this.m_window.LeftContent.Content,
            AppDisplayTarget.Right => this.m_window.RightContent.Content is FrameworkElement fe ? fe.DataContext : this.m_window.RightContent.Content,
            _ => throw new InvalidOperationException()
        };

        // close current if any
        if (old is IViewModel oldModel) {
            oldModel.UnloadViewModel(cancelled => {
                if (!cancelled) {
                    this.DoSwitch(target, model);
                    switchCallback?.Invoke(model);
                }
            }, destroyView);
        } else {
            DoSwitch(target, model);
            switchCallback?.Invoke(model);
        }

    }

    private void DoSwitch(AppDisplayTarget target, IViewModel? model) {

        // If single instance, notify of swapback
        if (model?.KeepAlive ?? false) {
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
        }

    }

    public void DestroyView(IViewModel model) { 
        
        // Remove from alive
        this.m_alive.Remove(model);

        // Get typename
        var typname = model.GetType().Name;

        // Remove from set of models
        if (this.m_models.ContainsKey(typname) && (this.m_models[typname]?.Equals(model) ?? false)) { 
            this.m_models.Remove(typname);
        }

    }

    public T? CreateDisplayIfNotFound<T>(Func<T> creator) where T : class, IViewModel
        => (this.m_models.TryGetValue(typeof(T).Name, out var model) ? model : (this.m_models[typeof(T).Name] = creator())) as T;

    public ModalControl? GetModalControl() 
        => this.m_window.ModalView;

    public ModalControl? GetRightsideModalControl()
        => this.m_window.RightModalView;

}
