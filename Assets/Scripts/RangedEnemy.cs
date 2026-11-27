using UnityEngine;

public class RangedEnemy : Enemy
{
    [Header("Death Drop")]
    [SerializeField] private GameObject energyPrefab;
    [SerializeField] private float energyLifetime = 7f;

    [Header("Ranged Attack")]
    [SerializeField] private EnemyBossBullet bulletPrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float attackRange = 8f;
    [SerializeField] private float fireRate = 1.5f;
    [SerializeField] private float bulletDamage = 10f;
    [SerializeField] private float bulletSpeed = 8f;

    private float nextShotTime;

    protected override void Update()
    {
        if (isDead || player == null)
            return;

        float distance = Vector2.Distance(transform.position, player.transform.position);

        if (distance > attackRange)
        {
            MoveToPlayer();
            return;
        }

        FlipEnemy();

        if (Time.time >= nextShotTime)
            ShootAtPlayer();
    }

    private void ShootAtPlayer()
    {
        if (bulletPrefab == null)
            return;

        Vector2 origin = firePoint != null
            ? (Vector2)firePoint.position
            : (Vector2)transform.position;
        Vector2 direction = ((Vector2)player.transform.position - origin).normalized;

        EnemyBossBullet bullet = Instantiate(
            bulletPrefab,
            origin,
            Quaternion.identity
        );

        bullet.SetDirection(direction);
        bullet.SetDamage(bulletDamage);
        bullet.SetMoveSpeed(bulletSpeed);

        nextShotTime = Time.time + Mathf.Max(0.05f, fireRate);
    }

    protected override void Die()
    {
        if (energyPrefab != null)
        {
            GameObject energy = Instantiate(
                energyPrefab,
                transform.position,
                Quaternion.identity
            );

            Destroy(energy, energyLifetime);
        }

        base.Die();
    }
}
