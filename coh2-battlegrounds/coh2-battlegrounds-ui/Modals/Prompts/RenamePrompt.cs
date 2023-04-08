using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;

using Battlegrounds.ErrorHandling.CommonExceptions;
using Battlegrounds.Locale;

namespace Battlegrounds.UI.Modals.Prompts;

/// <summary>
/// 
/// </summary>
/// <param name="sender"></param>
/// <param name="oldValue"></param>
/// <param name="newValue"></param>
public delegate void RenamePromptCallback(RenamePrompt sender, string oldValue, string newValue);

/// <summary>
/// 
/// </summary>
public sealed class RenamePrompt : INotifyPropertyChanged {

    private string m_newValue;

    /// <summary>
    /// 
    /// </summary>
    public string Title { get; }

    /// <summary>
    /// 
    /// </summary>
    public string OldValue { get; }

    /// <summary>
    /// 
    /// </summary>
    public string Value {
        get => this.m_newValue;
        set {
            this.m_newValue = value;
            this.PropertyChanged?.Invoke(this, new(nameof(Value)));
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public string AcceptTitle { get; }

    /// <summary>
    /// 
    /// </summary>
    public ICommand Accept { get; }


    public event PropertyChangedEventHandler? PropertyChanged;

    private RenamePrompt(LocaleKey title, RenamePromptCallback acceptCallback, LocaleKey acceptTitle, string oldvalue = "") {

        // Set title
        this.Title = title switch {
            LocaleValueKey lvk => lvk.Content,
            _ => BattlegroundsContext.Localize.GetString(title)
        };
        this.OldValue = oldvalue;

        // Set initial new value
        this.m_newValue = "";

        // Set accept button
        this.AcceptTitle = acceptTitle switch {
            LocaleValueKey lvk => lvk.Content,
            _ => BattlegroundsContext.Localize.GetString(acceptTitle)
        };

        // Set accept button callback
        this.Accept = new RelayCommand(() => acceptCallback(this, oldvalue, this.m_newValue));

    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="title"></param>
    /// <param name="acceptCallback"></param>
    /// <param name="acceptTitle"></param>
    /// <param name="oldvalue"></param>
    public static void Show(string title, RenamePromptCallback acceptCallback, string acceptTitle = "", string oldvalue = "") 
        => Show(new LocaleValueKey(title), acceptCallback, new LocaleValueKey(acceptTitle), oldvalue);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="control"></param>
    /// <param name="title"></param>
    /// <param name="acceptCallback"></param>
    /// <param name="acceptTitle"></param>
    /// <param name="oldvalue"></param>
    public static void Show(ModalControl control, string title, RenamePromptCallback acceptCallback, string acceptTitle = "", string oldvalue = "")
        => Show(control, new LocaleValueKey(title), acceptCallback, new LocaleValueKey(acceptTitle), oldvalue);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="title"></param>
    /// <param name="acceptCallback"></param>
    /// <param name="acceptTitle"></param>
    /// <param name="oldvalue"></param>
    /// <exception cref="ObjectNotFoundException"></exception>
    /// <exception cref="InvalidOperationException"></exception>
    public static void Show(LocaleKey title, RenamePromptCallback acceptCallback, LocaleKey acceptTitle, string oldvalue = "") {
        if (Application.Current.MainWindow is IMainWindow window) {
            Show(window.GetModalControl() ?? throw new ObjectNotFoundException("Failed to find main modal control."), title, acceptCallback, acceptTitle, oldvalue);
        } else {
            throw new InvalidOperationException("Failed to get window object with modal control access.");
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="control"></param>
    /// <param name="title"></param>
    /// <param name="acceptCallback"></param>
    /// <param name="acceptTitle"></param>
    /// <param name="oldvalue"></param>
    public static void Show(ModalControl control, LocaleKey title, RenamePromptCallback acceptCallback, LocaleKey acceptTitle, string oldvalue = "") {

        // Create dialog view model
        RenamePrompt dialog = new(title, (x, y, z) => {
            acceptCallback(x, y, z);
            control.CloseModal();
        }, acceptTitle, oldvalue);

        // Set background
        control.ModalMaskBehaviour = ModalBackgroundBehaviour.ExitWhenClicked;
        control.ShowModal(dialog);

    }


}
