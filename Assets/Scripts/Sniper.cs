using UnityEngine;

public class Sniper : Weapon
{
    protected override void Fire()
    {
        SpawnBullet();
    }
}
