using UnityEngine;

public class Shotgun : Weapon
{
    [SerializeField] private int bulletCount = 5;
    [SerializeField] private float spreadAngle = 30f;

    protected override void Fire()
    {
        for (int i = 0; i < bulletCount; i++)
        {
            float angle = bulletCount == 1
                ? 0
                : -spreadAngle / 2f
                  + spreadAngle * i / (bulletCount - 1);

            SpawnBullet(angle);
        }
    }
    public override void ApplyUpgrade(UpgradeType upgrade)
    {
        switch (upgrade)
        {
            case UpgradeType.ShotgunPelletCount:
                bulletCount += 1;
                break;

            case UpgradeType.ShotgunSpread:
                spreadAngle *= 0.8f;
                break;

            case UpgradeType.ShotgunAmmoCapacity:
                maxAmmo = Mathf.CeilToInt(maxAmmo * 1.2f);
                currentAmmo = maxAmmo;
                break;
        }
    }
}