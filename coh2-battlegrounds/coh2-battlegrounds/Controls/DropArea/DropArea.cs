using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;

namespace BattlegroundsApp.Controls.DropArea {

    //[ContentProperty(nameof(ControlContent))]
    public class DropArea : UserControl {

        //public static readonly DependencyProperty ControlContentProperty = DependencyProperty.Register(nameof(ControlContent), typeof(object), typeof(DropArea));

        //public object ControlContent {
        //    get { return GetValue(ControlContentProperty); }
        //    set { SetValue(ControlContentProperty, value); }
        //}


        static DropArea() {

            DefaultStyleKeyProperty.OverrideMetadata(typeof(DropArea), new FrameworkPropertyMetadata(typeof(DropArea)));

        }

    }
}
