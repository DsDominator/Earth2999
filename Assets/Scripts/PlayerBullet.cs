using UnityEngine;
using System.Collections.Generic;

public class PlayerBullet : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 10f;
    [SerializeField] private float timeDestroy = 2f;
    [SerializeField] private float damage = 10f;
    [SerializeField] private GameObject bloodPrefabs;
    private float finalDamage;

    [Header("Penetration")]
    [SerializeField] private int maxPenetration = 1;

    private int enemiesHit = 0;

    // Tránh một Enemy bị tính trúng nhiều lần
    private HashSet<Enemy> hitEnemies = new HashSet<Enemy>();

    void Start()
    {
        PlayerStats stats = FindAnyObjectByType<PlayerStats>();
        float multiplier = stats != null
            ? stats.damageMultiplier
            : 1f;

        finalDamage = damage * multiplier;
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
        if (!collision.CompareTag("Enemy")) return;

        Enemy enemy = collision.GetComponentInParent<Enemy>();

        if (enemy == null) return;

        // Bỏ qua Enemy đã bị viên đạn này bắn trúng
        if (hitEnemies.Contains(enemy)) return;

        hitEnemies.Add(enemy);

        enemy.TakeDamage(finalDamage);

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

        // Hủy đạn khi đạt giới hạn xuyên
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
