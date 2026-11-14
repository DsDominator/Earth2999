using UnityEngine;

public class Sniper : Weapon
{
    private int penetrationBonus = 0;

    public void UpgradePenetration()
    {
        penetrationBonus++;
        Debug.Log("Sniper Penetration Bonus: " + penetrationBonus);
    }

    protected override void Fire()
    {
        SpawnBullet();
    }

    // Truyền bonus xuyên địch vào viên đạn vừa tạo
    protected override void SpawnBullet(float angle = 0f)
    {
         if (firePos == null || bulletPrefabs == null)
            return;

        Quaternion bulletRotation = firePos.rotation *
            Quaternion.Euler(0, 0, angle);
        GameObject bulletObject =
        Instantiate(
            bulletPrefabs,
            firePos.position,
            bulletRotation
        );

        PlayerBullet bullet = bulletObject.GetComponent<PlayerBullet>();

        if (bullet != null)
        {
            bullet.UpgradePenetration(penetrationBonus);
        }
    }

    public override void ApplyUpgrade(UpgradeType upgrade)
    {
        switch (upgrade)
        {
            case UpgradeType.SniperPenetration:
                UpgradePenetration();
                break;

            case UpgradeType.SniperReloadSpeed:
                reloadTime *= 0.8f;
                break;

            case UpgradeType.SniperAmmoCapacity:
                maxAmmo = Mathf.CeilToInt(maxAmmo * 1.2f);
                currentAmmo = maxAmmo;
                break;
        }
    }
}