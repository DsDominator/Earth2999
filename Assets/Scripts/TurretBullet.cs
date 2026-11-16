
using UnityEngine;

public class TurretBullet : MonoBehaviour
{
    [Header("Bullet Settings")]
    [SerializeField] private float moveSpeed = 8f;
    [SerializeField] private float damage = 10f;
    [SerializeField] private float destroyTime = 5f;

    private Vector2 direction;
    private bool hasHit;

    private void Start()
    {
        direction = transform.right;

        Destroy(gameObject, destroyTime);
    }

    private void Update()
    {
        transform.position +=
            (Vector3)(direction * moveSpeed * Time.deltaTime);
    }

    public void SetDamage(float value)
    {
        damage = value;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (hasHit)
            return;

        if (collision.CompareTag("Player"))
        {
            Player target = collision.GetComponentInParent<Player>();

            if (target != null)
            {
                hasHit = true;
                target.TakeDamage(damage);
                Destroy(gameObject);
            }
        }
    }
}
