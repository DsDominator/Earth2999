
using UnityEngine;

public class FireDamageArea : MonoBehaviour
{
    [Header("Damage Settings")]
    [SerializeField] private float damage = 10f;
    [SerializeField] private float damageInterval = 0.5f;
    [SerializeField] private float duration = 3f;

    private float nextDamageTime;
    private float destroyTime;

    private void Start()
    {
        destroyTime = Time.time + duration;
    }

    public void Configure(
        float newDamage,
        float newInterval,
        float newDuration
    )
    {
        damage = newDamage;
        damageInterval = newInterval;
        duration = newDuration;
        destroyTime = Time.time + duration;
    }

    private void Update()
    {
        if (Time.time >= destroyTime)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player"))
            return;

        if (Time.time < nextDamageTime)
            return;

        Player player = collision.GetComponent<Player>();

        if (player == null)
            player = collision.GetComponentInParent<Player>();

        if (player != null)
        {
            player.TakeDamage(damage);
            nextDamageTime = Time.time + damageInterval;
        }
    }
}
