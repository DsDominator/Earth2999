
using UnityEngine;
using UnityEngine.UI;

public class Turret : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] private float maxHealth = 50f;
    private float currentHealth;

    [SerializeField] private Image hpBar;

    private bool isDestroyed;

    [Header("Target")]
    [SerializeField] private Transform player;
    [SerializeField] private float detectionRange = 10f;

    [Header("Shooting")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float fireRate = 1.5f;
    [SerializeField] private float bulletDamage = 10f;

    [Header("Lifetime")]
    [SerializeField] private float lifetime = 15f;
    [Header("Death Effect")]
    [SerializeField] private GameObject explosionPrefab;
    [SerializeField] private float explosionDuration = 0.6f;

    private float nextFireTime;

    private void Start()
    {
        currentHealth = maxHealth;
        UpdateHpBar();

        if (player == null)
        {
            GameObject target =
                GameObject.FindGameObjectWithTag("Player");

            if (target != null)
                player = target.transform;
        }

        Destroy(gameObject, lifetime);
    }

    private void Update()
    {
        UpdateHpBarRotation();
        if (isDestroyed || player == null)
            return;

        float distance = Vector2.Distance(
            transform.position,
            player.position
        );

        if (distance > detectionRange)
            return;

        AimAtPlayer();

        if (Time.time >= nextFireTime)
        {
            Shoot();
            nextFireTime = Time.time + fireRate;
        }
    }

    private void AimAtPlayer()
    {
        if (player == null)
            return;

        Vector2 direction =
            (player.position - transform.position).normalized;

        float angle = Mathf.Atan2(
            direction.y,
            direction.x
        ) * Mathf.Rad2Deg;

        // Sprite mặc định hướng sang phải
        transform.rotation = Quaternion.Euler(
            0f,
            0f,
            angle
        );
    }

    private void Shoot()
    {
        if (bulletPrefab == null || firePoint == null)
            return;

        GameObject bullet = Instantiate(
            bulletPrefab,
            firePoint.position,
            firePoint.rotation
        );

        TurretBullet bulletScript =
            bullet.GetComponent<TurretBullet>();

        if (bulletScript != null)
            bulletScript.SetDamage(bulletDamage);
    }

    // Nhận sát thương
    public void TakeDamage(float damage)
    {
        if (isDestroyed)
            return;

        currentHealth -= damage;
        currentHealth = Mathf.Max(currentHealth, 0);

        UpdateHpBar();

        if (currentHealth <= 0)
            Die();
    }

    // Cập nhật thanh máu
    private void UpdateHpBar()
    {
        if (hpBar != null && maxHealth > 0)
        {
            hpBar.fillAmount = currentHealth / maxHealth;
        }
    }

    // Tiêu diệt Turret
    private void Die()
    {
        if (isDestroyed)
            return;

        isDestroyed = true;

        Debug.Log("Turret Destroyed!");

        // Tạo hiệu ứng phát nổ tại vị trí Turret
        if (explosionPrefab != null)
        {
            GameObject explosion = Instantiate(
                explosionPrefab,
                transform.position,
                Quaternion.identity
            );

            Destroy(explosion, explosionDuration);
        }
        // Xóa Turret
        Destroy(gameObject);
    }
    private void UpdateHpBarRotation()
    {
        if (hpBar != null)
        {
            hpBar.rectTransform.rotation = Quaternion.identity;
        }
    }
}
