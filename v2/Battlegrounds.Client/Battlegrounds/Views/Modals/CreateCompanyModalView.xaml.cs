using Battlegrounds.Helpers;
using Battlegrounds.ViewModels.Modals;

namespace Battlegrounds.Views.Modals;

/// <summary>
/// Interaction logic for CreateCompanyModalView.xaml
/// </summary>
public partial class CreateCompanyModalView : DialogUserControl {
    
    public CreateCompanyModalView(CreateCompanyModalViewModel viewModel) : base(viewModel) {
        InitializeComponent();
    }

}
