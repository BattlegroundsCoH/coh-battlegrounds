using System;
using System.Collections.Generic;
using System.Windows;

using Battlegrounds.Functional;
using Battlegrounds.UI.Modals;

namespace Battlegrounds.UI;

using ViewModelType = Either<string, Type, IViewModel?>;

/// <summary>
/// Delegate for handling view switching in a <see cref="AppViewManager"/> instance.
/// </summary>
/// <param name="model">The new <see cref="IViewModel"/> instance being switched to.</param>
public delegate void OnViewSwitched(IViewModel? model);

/// <summary>
/// Application display state.
/// </summary>
public enum AppDisplayState {

    /// <summary>
    /// Display left and right panel content.
    /// </summary>
    LeftRight,

    /// <summary>
    /// Display only one item in full display mode.
    /// </summary>
    Full
}

/// <summary>
/// The display panel to target in the main application window.
/// </summary>
public enum AppDisplayTarget {

    /// <summary>
    /// Target left panel.
    /// </summary>
    Left,

    /// <summary>
    /// Target right panel.
    /// </summary>
    Right,

    /// <summary>
    /// Target the full display panel.
    /// </summary>
    Full

}

/// <summary>
/// Class responsible for managing the views being displayed in the application.
/// </summary>
public sealed class AppViewManager {

    private readonly HashSet<IViewModel> m_alive;
    private readonly Dictionary<string, IViewModel?> m_models;
    private readonly Dictionary<string, Func<object?, IViewModel>> m_viewFactories;
    private readonly IMainWindow m_window;

    /// <summary>
    /// Initialise a new <see cref="AppViewManager"/> instance attached to the specified <paramref name="window"/>.
    /// </summary>
    /// <param name="window">The window the new instance will manage.</param>
    public AppViewManager(IMainWindow window) {
        this.m_models = new();
        this.m_window = window;
        this.m_alive = new();
        this.m_viewFactories = new();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="displayState"></param>
    /// <param name="left"></param>
    /// <param name="right"></param>
    /// <exception cref="NotImplementedException"></exception>
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

    /// <summary>
    /// 
    /// </summary>
    /// <param name="view"></param>
    /// <returns></returns>
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

    /// <summary>
    /// 
    /// </summary>
    /// <param name="target"></param>
    /// <param name="display"></param>
    /// <param name="destroyView"></param>
    public void UpdateDisplay(AppDisplayTarget target, ViewModelType display, bool destroyView = true) {
        if (this.SolveDisplay(display) is IViewModel model) {
            this.UpdateDisplayInternal(target, model, destroyView, null);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="target"></param>
    /// <param name="destroyView"></param>
    public void UpdateDisplay<T>(AppDisplayTarget target, bool destroyView = true) where T : IViewModel
        => this.UpdateDisplay<T>(target, null, destroyView);

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="target"></param>
    /// <param name="switchCallback"></param>
    /// <param name="destroyView"></param>
    /// <exception cref="InvalidOperationException"></exception>
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

        // Grab containers
        var lhs = this.m_window.GetLeft();
        var rhs = this.m_window.GetRight();

        // Get old content
        var old = target switch {
            AppDisplayTarget.Left or AppDisplayTarget.Full => lhs.Content,
            AppDisplayTarget.Right => rhs.Content is FrameworkElement fe ? fe.DataContext : rhs.Content,
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

    /// <summary>
    /// Destroy the <see cref="IViewModel"/> and all visual resources associated with the model.
    /// </summary>
    /// <param name="model">The <see cref="IViewModel"/> to destroy.</param>
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

    /// <summary>
    /// Create a new <see cref="IViewModel"/> display if not already registered in the view manager.
    /// </summary>
    /// <typeparam name="T">The specific <see cref="IViewModel"/> type to instantiate.</typeparam>
    /// <param name="factory">The factory function to use if no preregistered display is found.</param>
    /// <returns>The found display or newly created display.</returns>
    public T? CreateDisplayIfNotFound<T>(Func<T> factory) where T : class, IViewModel
        => (this.m_models.TryGetValue(typeof(T).Name, out var model) ? model : (this.m_models[typeof(T).Name] = factory())) as T;

    /// <summary>
    /// Get the window-wide <see cref="ModalControl"/> instance.
    /// </summary>
    /// <returns>The <see cref="ModalControl"/> covering the entire window or <see langword="null"/> if none is defined.</returns>
    public ModalControl? GetModalControl()
        => this.m_window.GetModalControl();

    /// <summary>
    /// Get the right-panel <see cref="ModalControl"/> instance.
    /// </summary>
    /// <returns>The <see cref="ModalControl"/> covering the entire window or <see langword="null"/> if none is defined.</returns>
    public ModalControl? GetRightsideModalControl()
        => this.m_window.GetRightsideModalControl();

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="factory"></param>
    public void RegisterFactory<T>(Func<object?, T> factory) where T : IViewModel {
        IViewModel safeType(object? x) => factory(x);
        this.m_viewFactories[typeof(T).Name] = safeType;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="viewName"></param>
    /// <param name="factoryArgument"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public IViewModel CreateView(string viewName, object? factoryArgument = null) {
        if (this.m_viewFactories.TryGetValue(viewName, out var func)) {
            return func(factoryArgument);
        }
        throw new Exception();
    }

}
