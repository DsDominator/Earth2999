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
}