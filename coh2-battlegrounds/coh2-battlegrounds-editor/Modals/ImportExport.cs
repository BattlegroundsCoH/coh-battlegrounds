using System.ComponentModel;
using System.Windows;
using System.Windows.Input;

using Battlegrounds.UI.Modals;
using Battlegrounds.UI;

namespace Battlegrounds.Editor.Modals;

/// <summary>
/// 
/// </summary>
/// <param name="newValue"></param>
/// <param name="templateString"></param>
public delegate void ImportExportCallback(string newValue, string templateString);

/// <summary>
/// 
/// </summary>
public sealed class ImportExport : INotifyPropertyChanged {

    private bool m_isExportMode = false;

    /// <summary>
    /// 
    /// </summary>
    public string Title { get; }

    /// <summary>
    /// 
    /// </summary>
    public string OldValue { get; }

    private string m_newValue;

    /// <summary>
    /// 
    /// </summary>
    public string Value {
        get => this.m_newValue;
        set {
            this.m_newValue = value;
            this.PropertyChanged?.Invoke(this, new(nameof(Value)));
            this.PropertyChanged?.Invoke(this, new(nameof(CanOk)));
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public string OkButtonTitle { get; }

    /// <summary>
    /// 
    /// </summary>
    public ICommand OkButton { get; }

    private string m_templateStr;

    /// <summary>
    /// 
    /// </summary>
    public string TemplateString {
        get => this.m_templateStr;
        set {
            this.m_templateStr = value;
            this.PropertyChanged?.Invoke(this, new(nameof(TemplateString)));
            this.PropertyChanged?.Invoke(this, new(nameof(CanOk)));
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public bool CanOk {
        get {
            if (this.m_isExportMode)
                return true;
            else
                return this.TemplateString.Length > 0 && this.Value.Length > 0;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public bool IsNameReadonly => this.m_isExportMode;

    /// <summary>
    /// 
    /// </summary>
    public bool IsTemplateReadonly => this.m_isExportMode;

    public event PropertyChangedEventHandler? PropertyChanged;

    private ImportExport(string title, ImportExportCallback acceptCallback, string acceptTitle = "", string oldvalue = "", string templateString = "") {

        // Set title
        this.Title = title;
        this.OldValue = oldvalue;

        // Set template if there
        this.m_templateStr = templateString;

        // Set initial new value
        this.m_newValue = "";

        // Set accept button
        this.OkButtonTitle = acceptTitle;
        this.OkButton = new RelayCommand(() => acceptCallback(this.m_newValue, this.TemplateString));

    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="title"></param>
    /// <param name="templateString"></param>
    /// <param name="oldval"></param>
    public static void ShowExport(string title, string templateString, string oldval = "")
        => ShowExport(AppContext.GetModalControl(), title, templateString, oldval);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="control"></param>
    /// <param name="title"></param>
    /// <param name="templateString"></param>
    /// <param name="oldval"></param>
    public static void ShowExport(ModalControl control, string title, string templateString, string oldval = "") {

        // Create dialog view model
        ImportExport dialog = new(title, (_, tmpl) => {
            Clipboard.SetText(tmpl);
            control.CloseModal();
        }, "Export Company", oldval, templateString) {
            m_isExportMode = true
        };
        control.ModalMaskBehaviour = ModalBackgroundBehaviour.ExitWhenClicked;
        control.ShowModal(dialog);

    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="title"></param>
    /// <param name="resultCallback"></param>
    public static void ShowImport(string title, ImportExportCallback resultCallback)
        => ShowImport(AppContext.GetModalControl(), title, resultCallback);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="control"></param>
    /// <param name="title"></param>
    /// <param name="resultCallback"></param>
    public static void ShowImport(ModalControl control, string title, ImportExportCallback resultCallback) {

        // Create dialog view model
        ImportExport dialog = new(title, (a, b) => {
            resultCallback(a, b);
            control.CloseModal();
        }, "Import Company");
        control.ModalMaskBehaviour = ModalBackgroundBehaviour.ExitWhenClicked;
        control.ShowModal(dialog);

    }

}
