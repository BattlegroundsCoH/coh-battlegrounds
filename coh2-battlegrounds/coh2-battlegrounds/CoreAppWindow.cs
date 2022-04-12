namespace BattlegroundsApp {

    public abstract class CoreAppWindow : ViewStateMachine {

        public override void SetState(ViewState state) {
            base.SetState(state);
            this.Dispatcher.Invoke(() => {
                this.DataContext = state;
                this.InvalidateVisual();
            });
        }

    }

}
