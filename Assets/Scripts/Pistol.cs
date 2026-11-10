using UnityEngine;

public class Pistol : Weapon
{
    protected override void Fire()
    {
        SpawnBullet();
    }
}