using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

using Battlegrounds;
using Battlegrounds.Locale;

namespace BattlegroundsApp.Controls;

/// <summary>
/// Class representing a localised label using the <see cref="BattlegroundsInstance.Localize"/> instance to localise text. Extends <see cref="Label"/>.
/// </summary>
[DefaultProperty("LocKey")]
public class LocLabel : Label {

    /// <summary>
    /// 
    /// </summary>
    public static readonly DependencyProperty LocKeyProperty =
      DependencyProperty.Register("LocKey", typeof(object), typeof(LocLabel), new PropertyMetadata(string.Empty, OnLocaleKeyChanged));

    /// <summary>
    /// Get or set the localisation key to use.
    /// </summary>
    public object? LocKey {
        get => this.GetValue(LocKeyProperty);
        set {
            this.SetValue(LocKeyProperty, value);
            if (value is null) {
                this.Content = string.Empty;
                return;
            }
            if (BattlegroundsInstance.Localize is not null) {
                string str = value switch {
                    string s => BattlegroundsInstance.Localize.GetString(s),
                    LocaleKey k => BattlegroundsInstance.Localize.GetString(k),
                    _ => value.ToString()
                };
                if (this.UpperCaseAll) {
                    str = str.ToUpper();
                }
                this.Content = str;
            } else {
                
                this.Content = this.UpperCaseAll ? value.ToString().ToUpper() : value.ToString();
            }
        }
    }

    private static void OnLocaleKeyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
        if (d is LocLabel loc) {
            loc.LocKey = e.NewValue;
        } else {
            throw new InvalidOperationException();
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public static readonly DependencyProperty UpperCaseAllProperty =
      DependencyProperty.Register("UpperCaseAll", typeof(bool), typeof(LocLabel), new PropertyMetadata(false));

    /// <summary>
    /// Get or set if the label should be all upper case letters.
    /// </summary>
    public bool UpperCaseAll {
        get => (bool)this.GetValue(UpperCaseAllProperty);
        set => this.SetValue(UpperCaseAllProperty, value);
    }

}
