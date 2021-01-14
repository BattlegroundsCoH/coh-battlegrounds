using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace BattlegroundsApp.Dialogs.Service {

    public abstract class DialogWindowBase<T> : UserControl, INotifyPropertyChanged, IDialogViewModelBase<T> where T : Enum {

        public T Result { get; }

        public string Title { get; set; }

        public T DialogCloseDefault { get; set; }

        public T DialogResult { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        public virtual void CloseDialogWithResult(DialogWindow dialog, T result) {

            DialogResult = result;

            if (dialog != null) dialog.DialogResult = true;

        }

        public virtual T ShowDialog() {

            DialogWindow window = new DialogWindow {
                Content = this.Content
            };

            if (window.ShowDialog() == true) {
                return this.DialogResult;
            } else {
                return this.DialogCloseDefault;
            }

        }

        public virtual void OnPropertyChanged(string propertyName) 
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    }
}
