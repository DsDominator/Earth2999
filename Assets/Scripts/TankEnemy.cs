
using UnityEngine;
using System.Collections;

public class TankEnemy : Enemy
{
    [Header("Death Drop")]
    [SerializeField] private GameObject energyPrefab;
    [SerializeField] private float energyLifetime = 7f;

    [Header("Melee Attack")]
    [SerializeField] private float attackRange = 1.5f;
    [SerializeField] private float attackDamage = 25f;
    [SerializeField] private float attackCooldown = 2f;
    [SerializeField] private float attackWindup = 0.5f;

    [Header("Attack Hitbox")]
    [SerializeField] private Transform attackPoint;
    [SerializeField] private float attackRadius = 1f;

    [Header("Animation")]
    [SerializeField] private Animator animator;

    private float nextAttackTime;
    private bool isAttacking;
    private Coroutine attackCoroutine;

    private void Awake()
    {
        if (animator == null)
            animator = GetComponent<Animator>();
    }

    protected override void Update()
    {
        if (isDead || player == null)
            return;

        FlipEnemy();

        float distance = Vector2.Distance(
            transform.position,
            player.transform.position
        );

        // Player còn ở xa: tiếp tục đuổi theo
        if (distance > attackRange)
        {
            if (!isAttacking)
                MoveToPlayer();

            return;
        }

        // Đủ gần: dừng lại và tấn công
        if (!isAttacking && Time.time >= nextAttackTime)
        {
            attackCoroutine = StartCoroutine(AttackCoroutine());
        }
    }

    private IEnumerator AttackCoroutine()
    {
        isAttacking = true;

        if (animator != null)
            animator.SetTrigger("Attack");

        // Thời gian chuẩn bị vung đòn
        yield return new WaitForSeconds(attackWindup);

        if (isDead)
            yield break;

        DealDamage();

        nextAttackTime = Time.time + attackCooldown;

        isAttacking = false;
        attackCoroutine = null;
    }

    private void DealDamage()
    {
        Vector2 hitPosition = attackPoint != null
            ? attackPoint.position
            : transform.position;

        Collider2D[] hits = Physics2D.OverlapCircleAll(
            hitPosition,
            attackRadius
        );

        foreach (Collider2D hit in hits)
        {
            Player target = hit.GetComponentInParent<Player>();

            if (target != null)
            {
                target.TakeDamage(attackDamage);
                return;
            }
        }
    }

    protected override void Die()
    {
        if (attackCoroutine != null)
            StopCoroutine(attackCoroutine);

        isAttacking = false;

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

    private void OnDrawGizmosSelected()
    {
        Vector3 position = attackPoint != null
            ? attackPoint.position
            : transform.position;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(position, attackRadius);
    }
}
