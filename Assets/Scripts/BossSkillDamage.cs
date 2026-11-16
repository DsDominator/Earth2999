
using UnityEngine;

public class BossSkillDamage : MonoBehaviour
{
    [SerializeField] private float damage = 25f;
    [SerializeField] private float destroyTime = 2f;

    private void Start()
    {
        Destroy(gameObject, destroyTime);
    }

    public void SetDamage(float value)
    {
        damage = value;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            Player target = collision.GetComponent<Player>();

            if (target != null)
                target.TakeDamage(damage);
        }
    }
}
