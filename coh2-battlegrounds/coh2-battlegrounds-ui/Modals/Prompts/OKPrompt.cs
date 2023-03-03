using System.Windows.Input;

using Battlegrounds.Locale;

namespace Battlegrounds.UI.Modals.Prompts;

/// <summary>
/// 
/// </summary>
/// <param name="sender"></param>
/// <param name="result"></param>
public delegate void OKPromptCallback(OKPrompt sender, ModalDialogResult result);

/// <summary>
/// 
/// </summary>
public sealed class OKPrompt {

    /// <summary>
    /// Prompt callback that does nothing.
    /// </summary>
    public static readonly OKPromptCallback Nothing = new OKPromptCallback((_, _) => { });

    /// <summary>
    /// 
    /// </summary>
    public string DialogTitle { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public string DialogMessage { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public ICommand OKCommand { get; }

    private OKPrompt(OKPromptCallback resultCallback, LocaleKey title, LocaleKey message) {

        // Set OK command
        this.OKCommand = new RelayCommand(() => resultCallback?.Invoke(this, ModalDialogResult.Confirm));

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
    /// 
    /// </summary>
    /// <param name="resultCallback"></param>
    /// <param name="title"></param>
    /// <param name="message"></param>
    public static void Show(OKPromptCallback resultCallback, string title, string message)
        => Show(AppContext.GetModalControl(), resultCallback, new LocaleValueKey(title), new LocaleValueKey(message));

    /// <summary>
    /// 
    /// </summary>
    /// <param name="control"></param>
    /// <param name="resultCallback"></param>
    /// <param name="title"></param>
    /// <param name="message"></param>
    public static void Show(ModalControl control, OKPromptCallback resultCallback, string title, string message)
        => Show(control, resultCallback, new LocaleValueKey(title), new LocaleValueKey(message));

    /// <summary>
    /// 
    /// </summary>
    /// <param name="resultCallback"></param>
    /// <param name="title"></param>
    /// <param name="message"></param>
    public static void Show(OKPromptCallback resultCallback, LocaleKey title, LocaleKey message)
        => Show(AppContext.GetModalControl(), resultCallback, title, message);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="control"></param>
    /// <param name="resultCallback"></param>
    /// <param name="title"></param>
    /// <param name="message"></param>
    public static void Show(ModalControl control, OKPromptCallback resultCallback, LocaleKey title, LocaleKey message) {

        // Create dialog view model
        OKPrompt dialog = new((vm, result) => {
            resultCallback(vm, result);
            control.CloseModal();
        }, title, message);

        control.ModalMaskBehaviour = ModalBackgroundBehaviour.ExitWhenClicked;
        control.ShowModal(dialog);

    }

}
