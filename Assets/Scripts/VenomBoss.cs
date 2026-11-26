
using UnityEngine;
using System.Collections;

public class VenomBoss : BossEnemy
{
    [Header("Venom Bullet")]
    [SerializeField] private GameObject venomBulletPrefab;
    [SerializeField] private Transform venomFirePoint;
    [SerializeField] private float venomBulletSpeed = 12f;
    [SerializeField] private float venomBulletDamage = 10f;
    [SerializeField] private int bulletCount = 3;
    [SerializeField] private float spreadAngle = 30f;

    [Header("Poison Explosion")]
    [SerializeField] private GameObject poisonWarningPrefab;
    [SerializeField] private GameObject poisonExplosionPrefab;
    [SerializeField] private float explosionRadius = 3f;
    [SerializeField] private float explosionDamage = 25f;
    [SerializeField] private float warningTime = 1.5f;
    [SerializeField] private float explosionEffectDuration = 1f;

    [Header("Toxic Field")]
    [SerializeField] private GameObject toxicFieldPrefab;
    [SerializeField] private float toxicRadius = 3f;
    [SerializeField] private float toxicDamage = 5f;
    [SerializeField] private float toxicDuration = 5f;
    [SerializeField] private float toxicTickRate = 1f;

    [Header("Poison Minions")]
    [SerializeField] private GameObject poisonMinionPrefab;
    [SerializeField] private int minionCount = 3;
    [SerializeField] private float summonRadius = 2f;

    [Header("Rage Mode")]
    [SerializeField] private float rageHealthPercent = 0.3f;
    [SerializeField] private float rageCooldownMultiplier = 0.7f;
    [SerializeField] private float rageSpeedMultiplier = 1.3f;
    [SerializeField] private float rageDamageMultiplier = 1.5f;

    private bool isUsingSkill;
    private bool isRageMode;

    private GameObject activeWarning;
    private GameObject activeToxicField;

    protected override void Update()
    {
        base.Update();

        if (isDead)
            return;

        CheckRageMode();
    }

    // =====================================
    // RAGE MODE
    // =====================================

    private void CheckRageMode()
    {
        if (isRageMode || maxHp <= 0)
            return;

        if (currentHp <= maxHp * rageHealthPercent)
        {
            isRageMode = true;

            skillCooldown *= rageCooldownMultiplier;
            enemyMovespeed *= rageSpeedMultiplier;

            Debug.Log("VENOM BOSS ENTERED RAGE MODE!");
        }
    }

    // =====================================
    // RANDOM SKILL
    // =====================================

    protected override void UseSkill()
    {
        if (isDead || isUsingSkill)
            return;

        nextSkillTime = Time.time + skillCooldown;

        int randomSkill = Random.Range(0, 5);

        switch (randomSkill)
        {
            case 0:
                PlaySkillAnimation();
                StartCoroutine(VenomShot());
                break;

            case 1:
                PlaySkillAnimation();
                StartCoroutine(PoisonExplosion());
                break;

            case 2:
                PlaySkillAnimation();
                StartCoroutine(ToxicField());
                break;

            case 3:
                PlaySkillAnimation();
                SummonPoisonMinions();
                break;
            case 4:     
                PlaySkillAnimation();
                base.Teleport();
                break;
        }
    }

    // =====================================
    // 1. VENOM SHOT
    // =====================================

    private IEnumerator VenomShot()
    {
        isUsingSkill = true;

        Debug.Log("Venom Boss: Venom Shot!");

        if (player == null || venomBulletPrefab == null ||
            venomFirePoint == null)
        {
            isUsingSkill = false;
            yield break;
        }

        Vector2 direction =
            (player.transform.position - venomFirePoint.position).normalized;

        float damage = venomBulletDamage *
            (isRageMode ? rageDamageMultiplier : 1f);

        for (int i = 0; i < bulletCount; i++)
        {
            float angle = -spreadAngle / 2f;

            if (bulletCount > 1)
                angle += spreadAngle * i / (bulletCount - 1);

            Vector2 bulletDirection =
                Quaternion.Euler(0f, 0f, angle) * direction;

            GameObject bullet = Instantiate(
                venomBulletPrefab,
                venomFirePoint.position,
                Quaternion.identity
            );

            EnemyBossBullet venomBullet =
                bullet.GetComponent<EnemyBossBullet>();

            if (venomBullet != null)
            {
                venomBullet.SetDirection(bulletDirection);
                venomBullet.SetDamage(damage);
                venomBullet.SetMoveSpeed(venomBulletSpeed);
            }
        }

        yield return new WaitForSeconds(0.3f);

        isUsingSkill = false;
    }

    // =====================================
    // 2. POISON EXPLOSION
    // =====================================

    private IEnumerator PoisonExplosion()
    {
        isUsingSkill = true;

        Debug.Log("Venom Boss: Poison Explosion Charging!");

        Vector3 explosionPosition = transform.position;
        GameObject warning = null;

        if (poisonWarningPrefab != null)
        {
            warning = Instantiate(
                poisonWarningPrefab,
                explosionPosition,
                Quaternion.identity
            );

            warning.transform.localScale =
                Vector3.one * explosionRadius * 2f;

            activeWarning = warning;
        }

        yield return new WaitForSeconds(warningTime);

        if (isDead)
        {
            ClearWarning();
            yield break;
        }

        ClearWarning();

        if (poisonExplosionPrefab != null)
        {
            GameObject explosion = Instantiate(
                poisonExplosionPrefab,
                explosionPosition,
                Quaternion.identity
            );

            explosion.transform.localScale =
                Vector3.one * explosionRadius * 2f;

            Destroy(explosion, explosionEffectDuration);
        }

        if (player != null)
        {
            float distance = Vector2.Distance(
                explosionPosition,
                player.transform.position
            );

            if (distance <= explosionRadius)
            {
                float damage = explosionDamage *
                    (isRageMode ? rageDamageMultiplier : 1f);

                player.TakeDamage(damage);
            }
        }

        yield return new WaitForSeconds(explosionEffectDuration);

        isUsingSkill = false;

        Debug.Log("Venom Boss: Poison Explosion Finished!");
    }

    // =====================================
    // 3. TOXIC FIELD
    // =====================================

    private IEnumerator ToxicField()
    {
        isUsingSkill = true;

        Debug.Log("Venom Boss: Toxic Field!");

        if (player == null || toxicFieldPrefab == null)
        {
            isUsingSkill = false;
            yield break;
        }

        Vector3 fieldPosition = player.transform.position;

        activeToxicField = Instantiate(
            toxicFieldPrefab,
            fieldPosition,
            Quaternion.identity
        );

        activeToxicField.transform.localScale =
            Vector3.one * toxicRadius * 2f;

        float elapsedTime = 0f;

        while (elapsedTime < toxicDuration)
        {
            if (isDead)
            {
                ClearToxicField();
                yield break;
            }

            if (player != null)
            {
                float distance = Vector2.Distance(
                    fieldPosition,
                    player.transform.position
                );

                if (distance <= toxicRadius)
                {
                    float damage = toxicDamage *
                        (isRageMode ? rageDamageMultiplier : 1f);

                    player.TakeDamage(damage);
                }
            }

            yield return new WaitForSeconds(toxicTickRate);

            elapsedTime += toxicTickRate;
        }

        ClearToxicField();

        isUsingSkill = false;

        Debug.Log("Venom Boss: Toxic Field Finished!");
    }

    // =====================================
    // 4. SUMMON POISON MINIONS
    // =====================================

    private void SummonPoisonMinions()
    {
        isUsingSkill = true;

        Debug.Log("Venom Boss: Summon Poison Minions!");

        if (poisonMinionPrefab != null)
        {
            for (int i = 0; i < minionCount; i++)
            {
                Vector2 randomPosition =
                    Random.insideUnitCircle * summonRadius;

                Instantiate(
                    poisonMinionPrefab,
                    (Vector2)transform.position + randomPosition,
                    Quaternion.identity
                );
            }
        }

        isUsingSkill = false;
    }

    // =====================================
    // CLEAR WARNING
    // =====================================

    private void ClearWarning()
    {
        if (activeWarning != null)
        {
            Destroy(activeWarning);
            activeWarning = null;
        }
    }

    // =====================================
    // BOSS DEATH
    // =====================================

    protected override void Die()
    {
        if (isDead)
            return;

        // Xóa tất cả hiệu ứng của Boss
        ClearWarning();
        ClearToxicField();

        // Dừng các kỹ năng đang chạy
        StopAllCoroutines();

        // Xử lý Boss chết
        base.Die();

        Debug.Log("VENOM BOSS DEFEATED!");
    }

    private void OnDestroy()
    {
        ClearWarning();
    }
    private void ClearToxicField()
    {
        if (activeToxicField != null)
        {
            Destroy(activeToxicField);
            activeToxicField = null;
        }
    }
}
