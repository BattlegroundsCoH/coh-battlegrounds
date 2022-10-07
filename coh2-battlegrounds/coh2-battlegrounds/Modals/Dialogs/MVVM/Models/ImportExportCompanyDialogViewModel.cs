using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;

using Battlegrounds.UI;
using Battlegrounds.UI.Modals;

namespace BattlegroundsApp.Modals.Dialogs.MVVM.Models;

public class ImportExportCompanyDialogViewModel : INotifyPropertyChanged {

    private bool m_isExportMode = false;

    public string Title { get; }

    public string OldValue { get; }

    private string m_newValue;

    public string Value {
        get => this.m_newValue;
        set {
            this.m_newValue = value;
            this.PropertyChanged?.Invoke(this, new(nameof(Value)));
            this.PropertyChanged?.Invoke(this, new(nameof(CanOk)));
        }
    }

    public string OkButtonTitle { get; }

    public ICommand OkButton { get; }

    private string m_templateStr;

    public string TemplateString {
        get => this.m_templateStr;
        set {
            this.m_templateStr = value;
            this.PropertyChanged?.Invoke(this, new(nameof(TemplateString)));
            this.PropertyChanged?.Invoke(this, new(nameof(CanOk)));
        }
    }

    public bool CanOk {
        get {
            if (this.m_isExportMode)
                return true;
            else
                return this.TemplateString.Length > 0 && this.Value.Length > 0;
        }
    }

    public bool IsNameReadonly => this.m_isExportMode;

    public bool IsTemplateReadonly => this.m_isExportMode;

    public event PropertyChangedEventHandler? PropertyChanged;

    public ImportExportCompanyDialogViewModel(string title, Action<string, string> acceptCallback, string acceptTitle = "", string oldvalue = "", string templateString = "") {

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

    public static void ShowExport(ModalControl control, string title, string templateString, string oldval = "") {

        // Create dialog view model
        ImportExportCompanyDialogViewModel dialog = new(title, (_,tmpl) => {
            Clipboard.SetText(tmpl);
            control.CloseModal();
        }, "Export Company", oldval, templateString) {
            m_isExportMode = true
        };
        control.ModalMaskBehaviour = ModalBackgroundBehaviour.ExitWhenClicked;
        control.ShowModal(dialog);

    }

    public static void ShowImport(ModalControl control, string title, Action<string, string> resultCallback) {

        // Create dialog view model
        ImportExportCompanyDialogViewModel dialog = new(title, (a,b) => {
            resultCallback(a, b);
            control.CloseModal();
        }, "Import Company");
        control.ModalMaskBehaviour = ModalBackgroundBehaviour.ExitWhenClicked;
        control.ShowModal(dialog);

    }

}
