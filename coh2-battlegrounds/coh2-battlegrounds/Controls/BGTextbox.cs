using System;
using System.Windows.Controls;
using System.Windows.Input;

namespace BattlegroundsApp.Controls;

public class BGTextbox : TextBox {

    public event Action<object, KeyEventArgs> KeyDownEvent;

    protected override void OnKeyDown(KeyEventArgs e) {
        this.KeyDownEvent?.Invoke(this, e);
        base.OnKeyDown(e);
    }

}
