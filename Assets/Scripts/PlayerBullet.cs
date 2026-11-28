
using UnityEngine;
using System.Collections.Generic;

public class PlayerBullet : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 10f;
    [SerializeField] private float timeDestroy = 2f;
    [SerializeField] private float damage = 10f;
    [SerializeField] private GameObject bloodPrefabs;

    private float finalDamage;
    private bool isCritical;

    [Header("Penetration")]
    [SerializeField] private int maxPenetration = 1;

    private int enemiesHit = 0;

    // Theo dõi Enemy đã trúng đạn
    private HashSet<Enemy> hitEnemies = new HashSet<Enemy>();

    // Theo dõi Turret đã trúng đạn
    private HashSet<Turret> hitTurrets = new HashSet<Turret>();

    void Start()
    {
        PlayerStats stats = FindAnyObjectByType<PlayerStats>();

        float damageMultiplier = stats != null
            ? stats.damageMultiplier
            : 1f;

        finalDamage = damage * damageMultiplier;

        isCritical = stats != null && Random.value < stats.critChance;
        if (isCritical)
        {
            finalDamage *= stats.critDamageMultiplier;
        }
        Destroy(gameObject, timeDestroy);
    }

    void Update()
    {
        MoveBullet();
    }

    void MoveBullet()
    {
        transform.Translate(Vector2.right * moveSpeed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // =========================
        // HIT ENEMY
        // =========================

        if (collision.CompareTag("Enemy"))
        {
            Enemy enemy = collision.GetComponentInParent<Enemy>();

            if (enemy == null || hitEnemies.Contains(enemy))
                return;

            hitEnemies.Add(enemy);

            enemy.TakeDamage(finalDamage, isCritical);

            if (bloodPrefabs != null)
            {
                GameObject blood = Instantiate(
                    bloodPrefabs,
                    transform.position,
                    Quaternion.identity
                );

                Destroy(blood, 1f);
            }

            enemiesHit++;
        }

        // =========================
        // HIT TURRET
        // =========================

        else if (collision.CompareTag("Turret"))
        {
            Turret turret = collision.GetComponentInParent<Turret>();

            if (turret == null || hitTurrets.Contains(turret))
                return;

            hitTurrets.Add(turret);

            turret.TakeDamage(finalDamage);

            enemiesHit++;
        }
        else
        {
            return;
        }

        // =========================
        // PENETRATION
        // =========================

        if (enemiesHit >= maxPenetration)
        {
            Destroy(gameObject);
        }
    }

    public void UpgradePenetration(int amount)
    {
        maxPenetration += amount;
    }
}
