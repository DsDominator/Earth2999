using UnityEngine;

public class EnemyBossBullet : MonoBehaviour
{
    [Header("Bullet Settings")]
    [SerializeField] private float moveSpeed = 12f;
    [SerializeField] private float damage = 10f;
    [SerializeField] private float destroyTime = 5f;

    private Vector2 direction;

    private void Start()
    {
        Destroy(gameObject, destroyTime);
    }

    public void SetDirection(Vector2 newDirection)
    {
        direction = newDirection.normalized;

        // Xoay sprite theo hướng bay
        float angle = Mathf.Atan2(
            direction.y,
            direction.x
        ) * Mathf.Rad2Deg;

        transform.rotation = Quaternion.Euler(
            0f, 0f, angle
        );
    }

    private void Update()
    {
        transform.position +=
            (Vector3)(direction * moveSpeed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            Player target = collision.GetComponentInParent<Player>();

            if (target != null)
                target.TakeDamage(damage);

            Destroy(gameObject);
        }
    }
    public void SetDamage(float value)
    {
        damage = value;
    }

    public void SetMoveSpeed(float value)
    {
        moveSpeed = value;
    }
}