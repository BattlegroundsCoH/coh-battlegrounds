using System;
using System.Windows;
using System.Windows.Input;

using Battlegrounds.ErrorHandling.CommonExceptions;
using Battlegrounds.Locale;

namespace Battlegrounds.UI.Modals.Prompts;

/// <summary>
/// Callback delegate for handling the modal close event of a <see cref="YesNoPrompt"/>.
/// </summary>
/// <param name="sender">The <see cref="YesNoPrompt"/> that triggered the callback.</param>
/// <param name="dialogResult">The result value that triggered the callback.</param>
public delegate void YesNoPromptCallback(YesNoPrompt sender, ModalDialogResult dialogResult);

/// <summary>
/// Class representing a yes/no prompt.
/// </summary>
public class YesNoPrompt {

    /// <summary>
    /// Get the prompt title.
    /// </summary>
    public string DialogTitle { get; }

    /// <summary>
    /// Get the prompt description.
    /// </summary>
    public string DialogMessage { get; }

    /// <summary>
    /// Get the command to trigger when 'Yes' is pressed.
    /// </summary>
    public ICommand YesCommand { get; }

    /// <summary>
    /// Get the command to trigger when 'No' is pressed.
    /// </summary>
    public ICommand NoCommand { get; }

    private YesNoPrompt(YesNoPromptCallback resultCallback, LocaleKey title, LocaleKey message) {

        // Register commands
        this.YesCommand = new RelayCommand(() => resultCallback?.Invoke(this, ModalDialogResult.Confirm));
        this.NoCommand = new RelayCommand(() => resultCallback?.Invoke(this, ModalDialogResult.Cancel));

        // Set title
        this.DialogTitle = title switch {
            LocaleValueKey lvk => lvk.Content,
            _ => BattlegroundsInstance.Localize.GetString(title)
        };

        // Set message
        this.DialogMessage = message switch {
            LocaleValueKey lvk => lvk.Content,
            _ => BattlegroundsInstance.Localize.GetString(message)
        };

    }

    /// <summary>
    /// Show a <see cref="YesNoPrompt"/> prompt on the main modal view.
    /// </summary>
    /// <param name="resultCallback">The callback to invoke when an action is taken.</param>
    /// <param name="title">The constant title value to display.</param>
    /// <param name="message">The constant description value to display.</param>
    public static void Show(YesNoPromptCallback resultCallback, string title, string message)
        => Show(resultCallback, new LocaleValueKey(title), new LocaleValueKey(message));

    /// <summary>
    /// Show a <see cref="YesNoPrompt"/> prompt on the main modal view.
    /// </summary>
    /// <param name="resultCallback">The callback to invoke when an action is taken.</param>
    /// <param name="title">The title to display.</param>
    /// <param name="message">The description to display.</param>
    /// <exception cref="ObjectNotFoundException"></exception>
    /// <exception cref="InvalidOperationException"></exception>
    public static void Show(YesNoPromptCallback resultCallback, LocaleKey title, LocaleKey message) {
        if (Application.Current.MainWindow is IMainWindow window) {
            Show(window.GetModalControl() ?? throw new ObjectNotFoundException("Failed to find main modal control."), resultCallback, title, message);
        } else {
            throw new InvalidOperationException("Failed to get window object with modal control access.");
        }
    }

    /// <summary>
    /// Show a <see cref="YesNoPrompt"/> prompt on the specified modal view.
    /// </summary>
    /// <param name="control">The control to display prompt to.</param>
    /// <param name="resultCallback">The callback to invoke when an action is taken.</param>
    /// <param name="title">The constant title value to display.</param>
    /// <param name="message">The constant description value to display.</param>
    public static void Show(ModalControl control, YesNoPromptCallback resultCallback, string title, string message)
        => Show(control, resultCallback, new LocaleValueKey(title), new LocaleValueKey(message));

    /// <summary>
    /// Show a <see cref="YesNoPrompt"/> prompt on the specified modal view.
    /// </summary>
    /// <param name="control">The control to display prompt to.</param>
    /// <param name="resultCallback">The callback to invoke when an action is taken.</param>
    /// <param name="title">The title to display.</param>
    /// <param name="message">The description to display.</param>
    public static void Show(ModalControl control, YesNoPromptCallback resultCallback, LocaleKey title, LocaleKey message) {

        // Create dialog view model
        YesNoPrompt dialog = new((vm, result) => {
            resultCallback(vm, result);
            control.CloseModal();
        }, title, message);

        // Set behaviour and show the prompt
        control.ModalMaskBehaviour = ModalBackgroundBehaviour.None;
        control.ShowModal(dialog);

    }

}
