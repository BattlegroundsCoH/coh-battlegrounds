using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;

using Battlegrounds.UI.Modals;

namespace Battlegrounds.Editor.Modals;

/// <summary>
/// Interaction logic for SquadSettingsView.xaml
/// </summary>
public partial class SquadSettingsView : Modal {
    
    public SquadSettingsView() {
        InitializeComponent();
    }

    protected override void OnModalClosing(ModalCloseEventArgs closeArgs) {

        // Call base
        base.OnModalClosing(closeArgs);

        // Update slot
        if (this.DataContext is SquadSettings vm) {
            vm.OnClose();
        }

    }

    private void EditSquadName_Click(object sender, RoutedEventArgs e) {
        if (this.DataContext is not SquadSettings vm) {
            return;
        }
        if (this.EditName.Visibility is Visibility.Collapsed) { // IntelliSense may fail for some reason in this file!
            this.RunName.SetCurrentValue(Run.TextProperty, string.Empty);
            this.EditSquadName.Content = "Save"; // IntelliSense may fail for some reason in this file!
            this.EditName.Visibility = Visibility.Visible; // IntelliSense may fail for some reason in this file!
        } else {
            this.EditSquadName.Content = "Edit"; // IntelliSense may fail for some reason in this file!
            this.EditName.Visibility = Visibility.Collapsed; // IntelliSense may fail for some reason in this file!
            if (this.EditName.Text.Length > 0) { // IntelliSense may fail for some reason in this file!
                vm.SetCustomName(this.EditName.Text); // IntelliSense may fail for some reason in this file!
            }
            vm.RefreshName();
        }
    }

    private void EditName_KeyUp(object sender, KeyEventArgs e) {
        if (e.Key is Key.Enter) {
            this.EditSquadName_Click(this, new());
        }
    }

}
