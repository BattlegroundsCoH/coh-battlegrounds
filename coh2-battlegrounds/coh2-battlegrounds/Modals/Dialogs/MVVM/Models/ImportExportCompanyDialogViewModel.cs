using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

using BattlegroundsApp.Utilities;

namespace BattlegroundsApp.Modals.Dialogs.MVVM.Models;

public class ImportExportCompanyDialogViewModel : INotifyPropertyChanged {

    public string Title { get; }

    public string OldValue { get; }

    private string m_newValue;

    public string Value {
        get => this.m_newValue;
        set {
            this.m_newValue = value;
            this.PropertyChanged?.Invoke(this, new(nameof(Value)));
        }
    }

    public string OkButtonTitle { get; }

    public ICommand OkButton { get; }

    public string TemplateString { 
        get; 
        set; 
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public ImportExportCompanyDialogViewModel(string title, Action<string, string> acceptCallback, string acceptTitle = "", string oldvalue = "", string templateString = "") {

        // Set title
        this.Title = title;
        this.OldValue = oldvalue;

        // Set template if there
        this.TemplateString = templateString;

        // Set initial new value
        this.m_newValue = "";

        // Set accept button
        this.OkButtonTitle = acceptTitle;
        this.OkButton = new RelayCommand(() => acceptCallback(this.m_newValue, this.TemplateString));

    }

    public static void ShowExport(ModalControl control, string title, string templateString, string oldval = "") {

        // Create dialog view model
        ImportExportCompanyDialogViewModel dialog = new(title, (_,_) => { }, "Export Company", oldval, templateString);
        control.ModalMaskBehaviour = ModalBackgroundBehaviour.ExitWhenClicked;
        control.ShowModal(dialog);

    }

    public static void ShowImport(ModalControl control, string title, Action<string, string> resultCallback) {

        // Create dialog view model
        ImportExportCompanyDialogViewModel dialog = new(title, resultCallback, "Import Company");
        control.ModalMaskBehaviour = ModalBackgroundBehaviour.ExitWhenClicked;
        control.ShowModal(dialog);

    }

}
