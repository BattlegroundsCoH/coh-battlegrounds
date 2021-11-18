using System;
using System.Windows.Controls;
using System.Windows.Input;

using BattlegroundsApp.Utilities;

namespace BattlegroundsApp.Modals {

    public delegate void ModalDialogClosed<T>(ModalDialog sender, bool success, T value) where T : Enum;

    public enum ModalDialogResult {
        Confirm,
        Cancel
    }

    public class ModalDialog : Modal {

        public ICommand ConfirmCommand { get; set; }

        public ICommand CancelCommand { get; set; }

        public string Message { get; set; }

        public string Title { get; set; }

        private ModalDialog(UserControl dialog, string title, string message) {
            this.Content = dialog;
            this.Title = title;
            this.Message = message;
        }

        public static ModalDialog CreateModal<TEnum>(string title, string message, TEnum confirm, TEnum cancel, ModalDialogClosed<TEnum> modalDialogClosed)
            where TEnum : Enum {
            ModalDialogView root = new ModalDialogView();
            ModalDialog dialog = new(root, title, message);
            dialog.ConfirmCommand = new RelayCommand(() => {
                modalDialogClosed?.Invoke(dialog, true, confirm);
                dialog.CloseModal();
            });
            dialog.CancelCommand = new RelayCommand(() => {
                modalDialogClosed?.Invoke(dialog, false, cancel);
                dialog.CloseModal();
            });
            root.DataContext = dialog;
            return dialog;
        }

        public static ModalDialog CreateModal(string title, string message, ModalDialogClosed<ModalDialogResult> closed)
            => CreateModal(title, message, ModalDialogResult.Confirm, ModalDialogResult.Cancel, closed);

    }

}
