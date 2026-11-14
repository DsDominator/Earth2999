
using UnityEngine;

public enum WeaponType
{
    Pistol,
    Shotgun,
    Sniper
}

public enum UpgradeType
{
    PistolFireRate,
    PistolAmmoCapacity,
    PistolReloadSpeed,

    ShotgunPelletCount,
    ShotgunSpread,
    ShotgunAmmoCapacity,

    SniperPenetration,
    SniperReloadSpeed,
    SniperAmmoCapacity
}

[System.Serializable]
public class WeaponUpgrade
{
    public WeaponType weapon;
    public UpgradeType upgrade;

    [Header("Upgrade Information")]
    public string upgradeName;

    [TextArea(2, 4)]
    public string description;

    public Sprite icon;
}
