using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace BattlegroundsApp.Views.CampaignViews.Models {

    public interface ICampaignMapVisual {

        public UIElement VisualElement { get; }

        public void SetPosition(double x, double y, int z = 100) {
            this.VisualElement.SetValue(Canvas.LeftProperty, x);
            this.VisualElement.SetValue(Canvas.TopProperty, y);
            this.VisualElement.SetValue(Panel.ZIndexProperty, z);
        }

        public void GotoPosition(double x, double y)
            => this.GotoPosition(x, y, TimeSpan.FromSeconds(2));

        public void GotoPosition(double x, double y, TimeSpan time) {

            DoubleAnimation xAnim = new DoubleAnimation((double)this.VisualElement.GetValue(Canvas.LeftProperty), x, new Duration(time)) {
                RepeatBehavior = new RepeatBehavior(1),
            };
            xAnim.Freeze();

            DoubleAnimation yAnim = new DoubleAnimation((double)this.VisualElement.GetValue(Canvas.TopProperty), y, new Duration(time)) {
                RepeatBehavior = new RepeatBehavior(1),
            };
            yAnim.Freeze();

            this.VisualElement.BeginAnimation(Canvas.LeftProperty, xAnim, HandoffBehavior.Compose);
            this.VisualElement.BeginAnimation(Canvas.TopProperty, yAnim, HandoffBehavior.Compose);

        }

    }

}
