using System;
using System.ComponentModel;
using System.Windows.Input;

using BattlegroundsApp.Utilities;

namespace BattlegroundsApp.Modals.Dialogs.MVVM.Models;

public class RenameDialogViewModel : INotifyPropertyChanged {

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

    public string AcceptTitle { get; }

    public ICommand Accept { get; } 

    public event PropertyChangedEventHandler? PropertyChanged;

    public RenameDialogViewModel(string title, Action<string> acceptCallback, string acceptTitle = "", string oldvalue = "") {
        
        // Set title
        this.Title = title;
        this.OldValue = oldvalue;

        // Set initial new value
        this.m_newValue = "";

        // Set accept button
        this.AcceptTitle = acceptTitle;
        this.Accept = new RelayCommand(() => acceptCallback(this.m_newValue));

    }

    public static void ShowModal(ModalControl control, string title, Action<string> resultCallback, string acceptTitle = "", string oldval = "") {

        // Create dialog view model
        RenameDialogViewModel dialog = new(title, x => {
            resultCallback(x);
            control.CloseModal();
        }, acceptTitle, oldval);
        control.ModalMaskBehaviour = ModalBackgroundBehaviour.ExitWhenClicked;
        control.ShowModal(dialog);

    }

}
