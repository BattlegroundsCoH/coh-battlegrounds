using System.Windows.Input;

using Battlegrounds.Game.Database;
using Battlegrounds.Game.DataCompany;
using Battlegrounds.Locale;
using Battlegrounds.UI;

namespace Battlegrounds.Editor.Components;

/// <summary>
/// 
/// </summary>
/// <param name="item"></param>
/// <returns></returns>
public delegate bool CanEquipItem(EquipmentSlot item);

/// <summary>
/// 
/// </summary>
/// <param name="item"></param>
public delegate void EquipItemHandler(EquipmentSlot item);

/// <summary>
/// 
/// </summary>
public sealed class EquipmentSlot {

    private readonly CanEquipItem m_canEquipPredicate;

    /// <summary>
    /// 
    /// </summary>
    public CompanyItem Item { get; }

    /// <summary>
    /// 
    /// </summary>
    public string Portrait { get; }

    /// <summary>
    /// 
    /// </summary>
    public string Title { get; }

    /// <summary>
    /// 
    /// </summary>
    public string Army { get; }

    /// <summary>
    /// 
    /// </summary>
    public ICommand EquipClick { get; }

    /// <summary>
    /// 
    /// </summary>
    public bool CanEquip => this.m_canEquipPredicate(this);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="item"></param>
    /// <param name="onEquip"></param>
    /// <param name="canEquip"></param>
    public EquipmentSlot(CompanyItem item, EquipItemHandler onEquip, CanEquipItem canEquip) {

        // Set Item
        this.Item = item;

        // Grab portrait and title
        (this.Portrait, this.Title) = item.Item switch {
            IUIBlueprint ui => (ui.UI.Portrait, GameLocale.GetString(ui.UI.ScreenName)),
            _ => (string.Empty, string.Empty)
        };

        // Set army (if any)
        this.Army = item.Item switch {
            EntityBlueprint ebp => ebp.Faction?.Name,
            SquadBlueprint sbp => sbp.Army?.Name,
            _ => ""
        } ?? string.Empty;

        // Define equip click command
        this.EquipClick = new RelayCommand(() => onEquip(this));

        // Set can equip
        this.m_canEquipPredicate = canEquip;

    }

}
