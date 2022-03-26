using BattlegroundsApp.CompanyEditor.MVVM.Models;
using BattlegroundsApp.Modals;

namespace BattlegroundsApp.CompanyEditor.MVVM.Views;

/// <summary>
/// Interaction logic for SquadOptionsView.xaml
/// </summary>
public partial class SquadOptionsView : Modal {
    
    public SquadOptionsView() {
        InitializeComponent();
    }

    protected override void OnModalClosing(ModalCloseEventArgs closeArgs) {

        // Call base
        base.OnModalClosing(closeArgs);

        // Update slot
        if (this.DataContext is SquadOptionsViewModel vm) {
            vm.OnClose();
        }

    }

}
