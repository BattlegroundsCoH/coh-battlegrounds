using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace BattlegroundsApp.Views {
    /// <summary>
    /// Interaction logic for DivisionBuilderView.xaml
    /// </summary>
    public partial class CompanyBuilderView : ViewState, IStateMachine<ViewState> {

        public CompanyBuilderView() {
            InitializeComponent();
        }

        public ViewState State { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public StateChangeRequestHandler GetRequestHandler() => throw new NotImplementedException();

        public void SetState(ViewState state) => throw new NotImplementedException();

        public override void StateOnFocus() => throw new NotImplementedException();

        public override void StateOnLostFocus() => throw new NotImplementedException();

        bool IStateMachine<ViewState>.StateChangeRequest(object request) => throw new NotImplementedException();
    }
}
