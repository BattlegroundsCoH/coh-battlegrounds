using System.ComponentModel;

using Battlegrounds.Helpers;

using CommunityToolkit.Mvvm.Input;

namespace Battlegrounds.ViewModels.Modals;

public enum DialogResult {
    Confirm,
    Yes,
    No,
    Cancel
}

public sealed class DialogModalViewModel : INotifyModalDone, INotifyPropertyChanged {
    
    public event ModalDoneEventHandler? ModalDone;
    public event PropertyChangedEventHandler? PropertyChanged;

    private string _header = string.Empty;
    private string _description = string.Empty;

    private bool _isConfirmVisible = false; // Default to Confirm dialog visibility
    private bool _isCancelVisible = false;
    private bool _isYesVisible = false;
    private bool _isNoVisible = false;

    public string Header {
        get => _header;
        set {
            if (_header == value) return;
            _header = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Header)));
        }
    }

    public string Description {
        get => _description;
        set {
            if (_description == value) return;
            _description = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Description)));
        }
    }

    public IRelayCommand ConfirmCommand => new RelayCommand(() => OnModalDone(DialogResult.Confirm));

    public IRelayCommand YesCommand => new RelayCommand(() => OnModalDone(DialogResult.Yes));

    public IRelayCommand NoCommand => new RelayCommand(() => OnModalDone(DialogResult.No));

    public IRelayCommand CancelCommand => new RelayCommand(() => OnModalDone(DialogResult.Cancel));

    public bool IsConfirmVisible {
        get => _isConfirmVisible;
        set {
            if (_isConfirmVisible == value) return;
            _isConfirmVisible = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsConfirmVisible)));
        }
    }

    public bool IsCancelVisible {
        get => _isCancelVisible;
        set {
            if (_isCancelVisible == value) return;
            _isCancelVisible = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsCancelVisible)));
        }
    }

    public bool IsYesVisible {
        get => _isYesVisible;
        set {
            if (_isYesVisible == value) return;
            _isYesVisible = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsYesVisible)));
        }
    }

    public bool IsNoVisible {
        get => _isNoVisible;
        set {
            if (_isNoVisible == value) return;
            _isNoVisible = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsNoVisible)));
        }
    }

    public void SetType(DialogType type = DialogType.Confirm, string? header = null, string? description = null) {
        switch (type) {
            case DialogType.Confirm:
                Header = string.IsNullOrEmpty(header) ? "Confirm Action" : header;
                Description = string.IsNullOrEmpty(description) ? "Are you sure you want to proceed?" : description;
                break;
            case DialogType.YesNo:
                Header = string.IsNullOrEmpty(header) ? "Make a Choice" : header;
                Description = string.IsNullOrEmpty(description) ? "Please choose Yes or No." : description;
                break;
            case DialogType.YesNoCancel:
                Header = string.IsNullOrEmpty(header) ? "Decision Required" : header;
                Description = string.IsNullOrEmpty(description) ? "Please choose Yes, No, or Cancel." : description;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(type), type, null);
        }
        IsConfirmVisible = type is DialogType.Confirm;
        IsCancelVisible = type is DialogType.Confirm or DialogType.YesNoCancel;
        IsYesVisible = type is DialogType.YesNo or DialogType.YesNoCancel;
        IsNoVisible = type is DialogType.YesNo or DialogType.YesNoCancel;
    }

    private void OnModalDone(DialogResult result) {
        ModalDone?.Invoke(this, result);
    }

}
