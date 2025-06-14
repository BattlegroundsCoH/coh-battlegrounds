using Battlegrounds.Helpers;
using Battlegrounds.ViewModels.Modals;

namespace Battlegrounds.Views.Modals;

/// <summary>
/// Interaction logic for DialogModalView.xaml
/// </summary>
public partial class DialogModalView : DialogUserControl {

    public DialogModalView(DialogModalViewModel ctx) : base(ctx) {
        InitializeComponent();
    }

}
