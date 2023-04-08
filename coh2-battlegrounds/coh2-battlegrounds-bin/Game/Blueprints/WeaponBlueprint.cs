using System;

namespace Battlegrounds.Game.Blueprints;

public enum WeaponCategory {
    Undefined,
    Ballistic,
    Explosive,
    Flamethrower,
    SmallArms,
}

public enum WeaponSmallArmsType {
    Invalid,
    HeavyMachineGun,
    LightMachineGun,
    SubMachineGun,
    Pistol,
    Rifle
}

public enum WeaponBallisticType {
    Invalid,
    AntiTankGun,
    TankGun,
    InfantryAntiTankGun,
}

public enum WeaponExplosiveType {
    Invalid,
    Grenade,
    Artillery,
    Mine,
    Mortar
}

public enum WeaponOnFireCallbackType {
    None,
    ScarEvent_OnWeaponFired,
    ScarEvent_OnSecondaryWeaponFired,
    ScarEvent_OnTertiaryWeaponFired,
    ScarEvent_OnTopWeaponFired,
    ScarEvent_OnSmokeWeaponFired,
    ScarEvent_OnHEWeaponFired,
    ScarEvent_OnFlameWeaponFired,
    ScarEvent_OnSyncWeaponFired
}

/// <summary>
/// Class representing a blueprint for weapon types.
/// </summary>
public class WeaponBlueprint : Blueprint {

    /// <summary>
    /// The unique PropertyBagGroupdID assigned to this blueprint.
    /// </summary>
    public override BlueprintUID PBGID { get; }

    public override BlueprintType BlueprintType => BlueprintType.WBP;

    public override string Name { get; }

    /// <summary>
    /// Get the max damage that can be done by the weapon.
    /// </summary>
    public float MaxDamage { get; }

    /// <summary>
    /// Get the max range of the weapon.
    /// </summary>
    public float MaxRange { get; }

    /// <summary>
    /// Get the magazine size of the weapon (How many shot calculations before reloading)
    /// </summary>
    public int MagazineSize { get; }

    /// <summary>
    /// Get the amount of shot calculations to do in a burst.
    /// </summary>
    public float FireRate { get; }

    /// <summary>
    /// Get the weapon category
    /// </summary>
    public WeaponCategory Category { get; }

    /// <summary>
    /// Get the small arms type (Invalid if not applicable based on <see cref="Category"/>).
    /// </summary>
    public WeaponSmallArmsType SmallArmsType { get; }

    /// <summary>
    /// Get the ballistics type (Invalid if not applicable based on <see cref="Category"/>).
    /// </summary>
    public WeaponBallisticType BallisticType { get; }

    /// <summary>
    /// Get the explosive type (Invalid if not applicable based on <see cref="Category"/>).
    /// </summary>
    public WeaponExplosiveType ExplosiveType { get; }

    /// <summary>
    /// Get the ScarEvent that's triggered when this weapon fires.
    /// </summary>
    public WeaponOnFireCallbackType OnFireCallbackType { get; }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="name"></param>
    /// <param name="pbgid"></param>
    /// <param name="dmg"></param>
    /// <param name="range"></param>
    public WeaponBlueprint(string name,
        BlueprintUID pbgid,
        WeaponCategory category,
        Enum specificType,
        WeaponOnFireCallbackType callbackType,
        float dmg, float range, int msize, float frate) {

        // Set properties
        Name = name;
        PBGID = pbgid;
        MaxDamage = dmg;
        MaxRange = range;
        FireRate = frate;
        MagazineSize = msize;
        Category = category;
        switch (category) {
            case WeaponCategory.Ballistic:
                BallisticType = (WeaponBallisticType)specificType;
                break;
            case WeaponCategory.Explosive:
                ExplosiveType = (WeaponExplosiveType)specificType;
                break;
            case WeaponCategory.SmallArms:
                SmallArmsType = (WeaponSmallArmsType)specificType;
                break;
        }
        OnFireCallbackType = callbackType;

    }

}
