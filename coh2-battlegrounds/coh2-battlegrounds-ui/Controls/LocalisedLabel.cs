﻿using System;
using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows;

using Battlegrounds.Data;
using Battlegrounds.Functional;
using Battlegrounds.Misc.Locale;
using Battlegrounds.Locale;

namespace Battlegrounds.UI.Controls;

/// <summary>
/// Class representing a localised label using the <see cref="BattlegroundsContext.Localize"/> instance to localise text. Extends <see cref="Label"/>.
/// </summary>
[DefaultProperty(nameof(LocKey))]
[ContentProperty(nameof(LocKey))]
public sealed class LocalisedLabel : Label {

    /// <summary>
    /// Identifiers the <see cref="LocKey"/> property.
    /// </summary>
    public static readonly DependencyProperty LocKeyProperty =
      DependencyProperty.Register(nameof(LocKey), typeof(object), typeof(LocalisedLabel), new PropertyMetadata(string.Empty, OnLocaleKeyChanged));

    /// <summary>
    /// Get or set the localisation key to use.
    /// </summary>
    public object? LocKey {
        get => this.GetValue(LocKeyProperty);
        set {
            this.SetCurrentValue(LocKeyProperty, value);
            if (value is null) {
                this.Content = string.Empty;
                return;
            }
            this.RefreshDisplay();
        }
    }

    private static void OnLocaleKeyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
        if (d is LocalisedLabel loc) {
            loc.LocKey = e.NewValue;
        } else {
            throw new InvalidOperationException();
        }
    }

    /// <summary>
    /// Identifiers the <see cref="UpperCaseAll"/> property.
    /// </summary>
    public static readonly DependencyProperty UpperCaseAllProperty =
      DependencyProperty.Register(nameof(UpperCaseAll), typeof(bool), typeof(LocalisedLabel), new PropertyMetadata(false, OnCaseChanged));

    /// <summary>
    /// Get or set if the label should be all upper case letters.
    /// </summary>
    public bool UpperCaseAll {
        get => (bool)this.GetValue(UpperCaseAllProperty);
        set => this.SetCurrentValue(UpperCaseAllProperty, value);
    }

    private static void OnCaseChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e) {
        if (dependencyObject is LocalisedLabel loc) {
            loc.UpperCaseAll = (bool)e.NewValue;
            loc.RefreshDisplay();
        }
    }

    /// <summary>
    /// Identifiers the <see cref="Arguments"/> property.
    /// </summary>
    public static readonly DependencyProperty ArgumentsProperty =
      DependencyProperty.Register(nameof(Arguments), typeof(object), typeof(LocalisedLabel), new PropertyMetadata(null, OnArgumentsChanged));

    /// <summary>
    /// Get or set arguments to the locale key.
    /// </summary>
    public object? Arguments {
        get => this.GetValue(ArgumentsProperty);
        set => this.SetCurrentValue(ArgumentsProperty, value);
    }

    private static void OnArgumentsChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e) {
        if (dependencyObject is LocalisedLabel loc) {
            loc.Arguments = e.NewValue;
            loc.RefreshDisplay();
            if (e.NewValue is IObjectChanged objNew) {
                objNew.ObjectChanged += loc.OnArgumentsObjectChanged;
            } else if (e.NewValue is INotifyPropertyChanged changed) {
                changed.PropertyChanged += (a, b) => {
                    loc.RefreshDisplay();
                };
            }
            if (e.OldValue is IObjectChanged objOld) {
                objOld.ObjectChanged -= loc.OnArgumentsObjectChanged;
            }
        }
    }

    private void OnArgumentsObjectChanged(object sender, IObjectChanged val)
        => this.RefreshDisplay();

    private object[] GetArgs() {
        var args = this.Arguments;
        if (args is not null && BattlegroundsContext.Localize is not null) {
            if (args is Array array) {
                object[] vals = new object[array.Length];
                for (int i = 0; i < vals.Length; i++) {
                    vals[i] = array.GetValue(i) switch {
                        LocaleKey k => BattlegroundsContext.Localize.GetString(k),
                        string s => BattlegroundsContext.Localize.GetString(s),
                        object o => o,
                        _ => throw new InvalidProgramException()
                    };
                }
                return vals;
            } else if (args is LocaleKey k) {
                return new[] { BattlegroundsContext.Localize.GetString(k) };
            } else if (args is string s) {
                return new[] { BattlegroundsContext.Localize.GetString(s) ?? s };
            } else if (args is ILocaleArgumentsObject obj) {
                return obj.ToArgs();
            }
        }
        return Array.Empty<object>();
    }

    private void RefreshDisplay() {
        object value = this.GetValue(LocKeyProperty);
        if (BattlegroundsContext.Localize is Localize loc) {
            string str = value switch {
                string s => loc.GetString(s, this.GetArgs()),
                LocaleKey k => loc.GetString(k, this.GetArgs()),
                Either<string, LocaleKey> sk => loc.GetString(sk.SecondOption(new LocaleKey(sk.FirstOption("NotFound"))), this.GetArgs()),
                _ => loc.Converters.ContainsKey(value.GetType()) ? loc.GetObjectAsString(value) : (value.ToString() ?? string.Empty)
            };
            this.Content = this.UpperCaseAll ? str.ToUpper() : str;
        } else {
            this.Content = this.UpperCaseAll ? (value.ToString() ?? string.Empty).ToUpper() : value.ToString();
        }
    }

}
