using System.Windows.Controls;

using Battlegrounds;

namespace BattlegroundsApp.Controls;

/// <summary>
/// Class representing a localised label using the <see cref="BattlegroundsInstance.Localize"/> instance to localise text. Extends <see cref="Label"/>.
/// </summary>
public class LocLabel : Label {

    private string m_editorLocKey;

    /// <summary>
    /// Get or set the localisation key to use.
    /// </summary>
    public string? LocKey { 
        get => this.m_editorLocKey;
        set {
            string displayValue = this.m_editorLocKey = value;
            if (BattlegroundsInstance.Localize is not null) {
                displayValue = BattlegroundsInstance.Localize.GetString(this.m_editorLocKey);
            }
            this.Content = displayValue;
        }
    }

}
