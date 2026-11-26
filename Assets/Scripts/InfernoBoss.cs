
using UnityEngine;
using System.Collections;

public class InfernoBoss : BossEnemy
{
    [Header("Fireball")]
    [SerializeField] private GameObject fireballPrefab;
    [SerializeField] private float fireballDamage = 15f;
    [SerializeField] private float fireballCooldown = 1f;

    [Header("Flame Burst")]
    [SerializeField] private GameObject flameBurstPrefab;
    [SerializeField] private float flameBurstDamage = 20f;
    [SerializeField] private float flameBurstRadius = 3f;

    [Header("Lava Ground")]
    [SerializeField] private GameObject lavaGroundPrefab;
    [SerializeField] private float lavaDamage = 8f;
    [SerializeField] private float lavaDuration = 5f;

    [Header("Flame Trail")]
    [SerializeField] private GameObject flameTrailPrefab;
    [SerializeField] private GameObject flameWarningPrefab;
    [SerializeField] private float flameTrailDamage = 12f;
    [SerializeField] private float flameTrailLength = 8f;
    [SerializeField] private float flameTrailWarningTime = 1f;

    [Header("Meteor Rain")]
    [SerializeField] private GameObject meteorWarningPrefab;
    [SerializeField] private GameObject meteorPrefab;
    [SerializeField] private GameObject meteorImpactPrefab;

    [SerializeField] private int meteorCount = 5;
    [SerializeField] private float meteorDamage = 20f;
    [SerializeField] private float meteorRadius = 4f;

    [SerializeField] private float meteorWarningTime = 1.2f;
    [SerializeField] private float meteorFallHeight = 5f;
    [SerializeField] private float meteorFallHorizontalOffset = 4f;
    [SerializeField] private float meteorFallDuration = 0.6f;
    [Header("Skill Settings")]
    [SerializeField] private float skillCooldownTime = 3f;

    [Header("Inferno Rage")]
    [SerializeField] private float rageHealthPercent = 0.3f;
    [SerializeField] private float rageSpeedMultiplier = 1.5f;
    [SerializeField] private float rageDamageMultiplier = 1.5f;
    [Header("Flame Trail Settings")]
    [SerializeField] private bool flipFlameTrailDirection = false;

    private bool isRageMode = false;
    private float originalMoveSpeed;
    private Coroutine meteorCoroutine;

    private GameObject activeFlameTrail;
    private GameObject activeFlameWarning;

    private readonly System.Collections.Generic.List<GameObject>
        activeEffects = new System.Collections.Generic.List<GameObject>();

    // =====================================
    // START
    // =====================================

    protected override void Start()
    {
        base.Start();

        originalMoveSpeed = enemyMovespeed;
    }

    // =====================================
    // UPDATE
    // =====================================

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
        if (isRageMode)
            return;

        if (currentHp <= maxHp * rageHealthPercent)
        {
            isRageMode = true;

            enemyMovespeed *= rageSpeedMultiplier;

            Debug.Log("INFERNO LORD: RAGE MODE!");

            // Có thể thêm hiệu ứng Rage tại đây.
        }
    }

    private float GetDamage(float damage)
    {
        return damage * (isRageMode ? rageDamageMultiplier : 1f);
    }

    // =====================================
    // RANDOM SKILL
    // =====================================

    protected override void UseSkill()
    {
        if (isDead || player == null)
            return;

        nextSkillTime = Time.time + skillCooldownTime;

        int skill = Random.Range(0, 5);

        switch (skill)
        {
            case 0:
                PlaySkillAnimation();
                Fireball();
                break;

            case 1:
                PlaySkillAnimation();
                base.Teleport();
                FlameBurst();
                break;

            case 2:
                PlaySkillAnimation();
                LavaGround();
                break;

            case 3:     
                PlaySkillAnimation();
                MeteorRain();
                break;

            case 4:
                PlaySkillAnimation();
                FlameTrail();
                break;
        }
    }

    // =====================================
    // 1. FIREBALL
    // =====================================

    private void Fireball()
    {
        if (fireballPrefab == null || player == null)
            return;

        Vector2 direction =
            ((Vector2)player.transform.position -
             (Vector2)transform.position).normalized;

        GameObject bullet = Instantiate(
            fireballPrefab,
            transform.position,
            Quaternion.identity
        );

        EnemyBossBullet bulletScript =
            bullet.GetComponent<EnemyBossBullet>();

        if (bulletScript != null)
        {
            bulletScript.SetDirection(direction);
            bulletScript.SetDamage(GetDamage(fireballDamage));
        }
    }

    // =====================================
    // 2. FLAME BURST
    // =====================================

    private void FlameBurst()
    {
        if (flameBurstPrefab == null)
            return;

        GameObject effect = Instantiate(
            flameBurstPrefab,
            transform.position,
            Quaternion.identity
        );

        RegisterEffect(effect);

        FireDamageArea damageArea =
            effect.GetComponent<FireDamageArea>();

        if (damageArea != null)
        {
            damageArea.Configure(
                GetDamage(flameBurstDamage),
                0.5f,
                1.5f
            );
        }
    }

    // =====================================
    // 3. LAVA GROUND
    // =====================================

    private void LavaGround()
    {
        if (lavaGroundPrefab == null || player == null)
            return;

        Vector2 spawnPosition = player.transform.position;

        GameObject lava = Instantiate(
            lavaGroundPrefab,
            spawnPosition,
            Quaternion.identity
        );

        RegisterEffect(lava);

        FireDamageArea damageArea =
            lava.GetComponent<FireDamageArea>();

        if (damageArea != null)
        {
            damageArea.Configure(
                GetDamage(lavaDamage),
                0.5f,
                lavaDuration
            );
        }
    }

    // =====================================
    // 4. METEOR RAIN
    // =====================================

    private void MeteorRain()
    {
        if (player == null || meteorCoroutine != null)
            return;

        if (meteorWarningPrefab == null ||
            meteorPrefab == null ||
            meteorImpactPrefab == null)
        {
            Debug.LogWarning("Inferno Boss: Chưa gán đủ 3 Meteor Prefabs!");
            return;
        }

        meteorCoroutine = StartCoroutine(MeteorRainCoroutine());
    }

    private IEnumerator MeteorRainCoroutine()
    {
        for (int i = 0; i < meteorCount; i++)
        {
            if (isDead || player == null)
                break;

            // Chọn vị trí rơi gần Player
            Vector2 randomOffset =
                Random.insideUnitCircle * meteorRadius;

            Vector3 targetPosition =
                player.transform.position + (Vector3)randomOffset;

            // =====================================
            // 1. METEOR WARNING
            // =====================================

            GameObject warning = Instantiate(
                meteorWarningPrefab,
                targetPosition,
                Quaternion.identity
            );

            RegisterEffect(warning);

            yield return new WaitForSeconds(meteorWarningTime);

            if (warning != null)
            {
                activeEffects.Remove(warning);
                Destroy(warning);
            }

            if (isDead)
                break;

            // =====================================
            // 2. METEOR FALL
            // =====================================

            // Thiên thạch xuất hiện phía trên bên phải
            Vector3 startPosition = targetPosition
                + Vector3.up * meteorFallHeight
                + Vector3.right * meteorFallHorizontalOffset;

            GameObject meteor = Instantiate(
                meteorPrefab,
                startPosition,
                Quaternion.identity
            );

            RegisterEffect(meteor);

            float elapsed = 0f;

            while (elapsed < meteorFallDuration)
            {
                if (isDead)
                    break;

                elapsed += Time.deltaTime;

                float progress = Mathf.Clamp01(
                    elapsed / meteorFallDuration
                );

                meteor.transform.position = Vector3.Lerp(
                    startPosition,
                    targetPosition,
                    progress
                );

                yield return null;
            }

            if (isDead)
                break;

            // Đảm bảo thiên thạch tới đúng điểm va chạm
            if (meteor != null)
            {
                activeEffects.Remove(meteor);
                Destroy(meteor);
            }

            // =====================================
            // 3. METEOR IMPACT
            // =====================================

            GameObject impact = Instantiate(
                meteorImpactPrefab,
                targetPosition,
                Quaternion.identity
            );

            RegisterEffect(impact);

            FireDamageArea damageArea =
                impact.GetComponent<FireDamageArea>();

            if (damageArea != null)
            {
                damageArea.Configure(
                    GetDamage(meteorDamage),
                    0.5f,
                    1f
                );
            }

            // Khoảng nghỉ trước thiên thạch tiếp theo
            yield return new WaitForSeconds(0.35f);
        }

        meteorCoroutine = null;
    }

    // =====================================
    // 5. FLAME TRAIL
    // =====================================

    private void FlameTrail()
    {
        if (player == null || flameTrailPrefab == null)
            return;

        Vector2 direction =
            ((Vector2)player.transform.position -
             (Vector2)transform.position).normalized;

        StartCoroutine(FlameTrailCoroutine(direction));
    }

    private IEnumerator FlameTrailCoroutine(Vector2 direction)
    {
        if (flameWarningPrefab != null)
        {
            Vector3 warningPosition =
                transform.position +
                (Vector3)(direction * flameTrailLength / 2f);

            activeFlameWarning = Instantiate(
                flameWarningPrefab,
                warningPosition,
                Quaternion.identity
            );

            float angle = Mathf.Atan2(
                direction.y,
                direction.x
            ) * Mathf.Rad2Deg;

            activeFlameWarning.transform.rotation =
                Quaternion.Euler(0, 0, angle);

            RegisterEffect(activeFlameWarning);
        }

        yield return new WaitForSeconds(flameTrailWarningTime);

        if (activeFlameWarning != null)
        {
            activeEffects.Remove(activeFlameWarning);
            Destroy(activeFlameWarning);
            activeFlameWarning = null;
        }

        if (isDead || flameTrailPrefab == null)
            yield break;

        Vector3 trailPosition =
            transform.position +
            (Vector3)(direction * flameTrailLength / 2f);

        float trailAngle = Mathf.Atan2(
            direction.y,
            direction.x
        ) * Mathf.Rad2Deg;

        if (flipFlameTrailDirection)
        {
            trailAngle += 180f;
        }

        activeFlameTrail = Instantiate(
            flameTrailPrefab,
            trailPosition,
            Quaternion.Euler(0, 0, trailAngle)
        );

        RegisterEffect(activeFlameTrail);

        FireDamageArea damageArea =
            activeFlameTrail.GetComponent<FireDamageArea>();

        if (damageArea != null)
        {
            damageArea.Configure(
                GetDamage(flameTrailDamage),
                0.5f,
                5f
            );
        }

        activeFlameTrail = null;
    }

    // =====================================
    // REGISTER EFFECT
    // =====================================

    private void RegisterEffect(GameObject effect)
    {
        if (effect != null)
            activeEffects.Add(effect);
    }

    // =====================================
    // CLEAN UP WHEN BOSS DIES
    // =====================================

    protected override void Die()
    {
        if (isDead)
            return;

        if (meteorCoroutine != null)
        {
            StopCoroutine(meteorCoroutine);
            meteorCoroutine = null;
        }

        StopAllCoroutines();

        foreach (GameObject effect in activeEffects)
        {
            if (effect != null)
                Destroy(effect);
        }

        activeEffects.Clear();

        base.Die();
    }
}
