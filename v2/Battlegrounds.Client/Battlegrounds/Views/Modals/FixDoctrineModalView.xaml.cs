using Battlegrounds.Helpers;
using Battlegrounds.ViewModels.Modals;

namespace Battlegrounds.Views.Modals;

/// <summary>
/// Interaction logic for FixDoctrineModalView.xaml
/// </summary>
public partial class FixDoctrineModalView : DialogUserControl {

    public FixDoctrineModalView(FixDoctrineModalViewModel viewModel) : base(viewModel) {
        InitializeComponent();
    }

}
