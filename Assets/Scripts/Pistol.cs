using UnityEngine;

public class Pistol : Weapon
{
    protected override void Fire()
    {
        SpawnBullet();
    }
    public override void ApplyUpgrade(UpgradeType upgrade)
    {
        switch (upgrade)
        {
            case UpgradeType.PistolFireRate:
                shotDelay *= 0.85f;
                break;

            case UpgradeType.PistolAmmoCapacity:
                maxAmmo = Mathf.CeilToInt(maxAmmo * 1.2f);
                currentAmmo = maxAmmo;
                break;

            case UpgradeType.PistolReloadSpeed:
                reloadTime *= 0.8f;
                break;
        }
    }
}