using System;
using System.ComponentModel;

namespace BattlegroundsApp.Dialogs.Service {

    public abstract class DialogControlBase<T> : INotifyPropertyChanged, IDialogViewModelBase<T> where T : Enum {

        public T Result { get; }

        public string Title { get; set; }

        public T DialogCloseDefault { get; set; }

        public T DialogResult { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        public virtual void CloseDialogWithResult(DialogWindow dialog, T result) {
            this.DialogResult = result;
            if (dialog != null) {
                dialog.DialogResult = true;
            }
        }

        public virtual T ShowDialog() {

            DialogWindow window = new DialogWindow {
                Title = this.Title,
                DataContext = this,
            };

            if (window.ShowDialog() == true) {
                return this.DialogResult;
            } else {
                return this.DialogCloseDefault;
            }

        }

        public virtual void OnPropertyChanged(string propertyName)
            => this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    }
}
