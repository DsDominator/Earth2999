
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ThunderBoss : BossEnemy
{
    [Header("Thunder Shot")]
    [SerializeField] private GameObject thunderBulletPrefab;
    [SerializeField] private Transform thunderFirePoint;
    [SerializeField] private float thunderBulletSpeed = 15f;
    [SerializeField] private float thunderDamage = 12f;
    [SerializeField] private int bulletCount = 5;
    [SerializeField] private float spreadAngle = 60f;

    [Header("Chain Lightning")]
    [SerializeField] private GameObject chainWarningPrefab;
    [SerializeField] private GameObject chainLightningPrefab;
    [SerializeField] private float chainDamage = 25f;
    [SerializeField] private float chainRadius = 2f;
    [SerializeField] private float chainWarningTime = 0.8f;

    [Header("Lightning Strike")]
    [SerializeField] private GameObject lightningWarningPrefab;
    [SerializeField] private GameObject lightningStrikePrefab;
    [SerializeField] private float strikeDamage = 30f;
    [SerializeField] private float strikeRadius = 2f;
    [SerializeField] private float strikeWarningTime = 1.5f;
    [SerializeField] private float strikeEffectDuration = 1f;

    [Header("Electric Field")]
    [SerializeField] private GameObject electricFieldPrefab;
    [SerializeField] private float electricRadius = 3f;
    [SerializeField] private float electricDamage = 5f;
    [SerializeField] private float electricDuration = 5f;
    [SerializeField] private float electricTickRate = 1f;

    [Header("Rage Mode")]
    [SerializeField] private float rageHealthPercent = 0.3f;
    [SerializeField] private float rageCooldownMultiplier = 0.7f;
    [SerializeField] private float rageSpeedMultiplier = 1.3f;
    [SerializeField] private float rageDamageMultiplier = 1.5f;

    private bool isUsingSkill;
    private bool isRageMode;

    // Quản lý các hiệu ứng do Boss tạo ra
    private GameObject activeWarning;
    private GameObject activeStrikeEffect;
    private GameObject activeChainEffect;
    private GameObject activeElectricField;

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
        if (isRageMode || maxHp <= 0)
            return;

        if (currentHp <= maxHp * rageHealthPercent)
        {
            isRageMode = true;

            skillCooldown *= rageCooldownMultiplier;
            enemyMovespeed *= rageSpeedMultiplier;

            Debug.Log("THUNDER BOSS ENTERED RAGE MODE!");
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

        int randomSkill = Random.Range(0, 4);

        switch (randomSkill)
        {
            case 0:
                PlaySkillAnimation();
                StartCoroutine(ThunderShot());
                break;

            case 1:
                PlaySkillAnimation();
                StartCoroutine(ChainLightning());
                break;

            case 2:
                PlaySkillAnimation();
                StartCoroutine(LightningStrike());
                break;

            case 3:
                PlaySkillAnimation();
                StartCoroutine(ElectricField());
                break;
        }
    }

    // =====================================
    // 1. THUNDER SHOT
    // =====================================

    private IEnumerator ThunderShot()
    {
        isUsingSkill = true;

        Debug.Log("Thunder Boss: Thunder Shot!");

        if (player == null || thunderBulletPrefab == null ||
            thunderFirePoint == null)
        {
            isUsingSkill = false;
            yield break;
        }

        Vector2 direction =
            (player.transform.position - thunderFirePoint.position).normalized;

        float damage = thunderDamage *
            (isRageMode ? rageDamageMultiplier : 1f);

        for (int i = 0; i < bulletCount; i++)
        {
            float angle = -spreadAngle / 2f;

            if (bulletCount > 1)
                angle += spreadAngle * i / (bulletCount - 1);

            Vector2 bulletDirection =
                Quaternion.Euler(0f, 0f, angle) * direction;

            GameObject bullet = Instantiate(
                thunderBulletPrefab,
                thunderFirePoint.position,
                Quaternion.identity
            );

            EnemyBossBullet thunderBullet =
                bullet.GetComponent<EnemyBossBullet>();

            if (thunderBullet != null)
            {
                thunderBullet.SetDirection(bulletDirection);
                thunderBullet.SetDamage(damage);
                thunderBullet.SetMoveSpeed(thunderBulletSpeed);
            }
        }

        yield return new WaitForSeconds(0.3f);

        isUsingSkill = false;
    }

    // =====================================
    // 2. CHAIN LIGHTNING
    // =====================================

    private IEnumerator ChainLightning()
    {
        isUsingSkill = true;

        Debug.Log("Thunder Boss: Chain Lightning!");

        if (player == null)
        {
            isUsingSkill = false;
            yield break;
        }

        // Lưu vị trí Player tại thời điểm bắt đầu
        Vector3 targetPosition = player.transform.position;

        // Tạo cảnh báo
        if (chainWarningPrefab != null)
        {
            activeWarning = Instantiate(
                chainWarningPrefab,
                targetPosition,
                Quaternion.identity
            );

            activeWarning.transform.localScale =
                Vector3.one * chainRadius * 2f;
        }

        yield return new WaitForSeconds(chainWarningTime);

        if (isDead)
        {
            ClearWarning();
            yield break;
        }

        ClearWarning();

        // Tạo hiệu ứng sét
        if (chainLightningPrefab != null)
        {
            activeChainEffect = Instantiate(
                chainLightningPrefab,
                targetPosition,
                Quaternion.identity
            );

            Destroy(activeChainEffect, 1f);
        }

        // Gây sát thương
        if (player != null)
        {
            float distance = Vector2.Distance(
                targetPosition,
                player.transform.position
            );

            if (distance <= chainRadius)
            {
                float damage = chainDamage *
                    (isRageMode ? rageDamageMultiplier : 1f);

                player.TakeDamage(damage);
            }
        }

        yield return new WaitForSeconds(0.5f);

        isUsingSkill = false;

        Debug.Log("Thunder Boss: Chain Lightning Finished!");
    }

    // =====================================
    // 3. LIGHTNING STRIKE
    // =====================================

    private IEnumerator LightningStrike()
    {
        isUsingSkill = true;

        Debug.Log("Thunder Boss: Lightning Strike Charging!");

        if (player == null)
        {
            isUsingSkill = false;
            yield break;
        }

        // Lưu vị trí Player để Player có cơ hội né
        Vector3 strikePosition = player.transform.position;

        if (lightningWarningPrefab != null)
        {
            activeWarning = Instantiate(
                lightningWarningPrefab,
                strikePosition,
                Quaternion.identity
            );

            activeWarning.transform.localScale =
                Vector3.one * strikeRadius * 2f;
        }

        yield return new WaitForSeconds(strikeWarningTime);

        if (isDead)
        {
            ClearWarning();
            yield break;
        }

        ClearWarning();

        Debug.Log("Thunder Boss: Lightning Strike Impact!");

        if (lightningStrikePrefab != null)
        {
            activeStrikeEffect = Instantiate(
                lightningStrikePrefab,
                strikePosition,
                Quaternion.identity
            );

            activeStrikeEffect.transform.localScale =
                Vector3.one * strikeRadius * 2f;

            Destroy(activeStrikeEffect, strikeEffectDuration);
        }

        // Kiểm tra Player có nằm trong vùng sét không
        if (player != null)
        {
            float distance = Vector2.Distance(
                strikePosition,
                player.transform.position
            );

            if (distance <= strikeRadius)
            {
                float damage = strikeDamage *
                    (isRageMode ? rageDamageMultiplier : 1f);

                player.TakeDamage(damage);
            }
        }

        yield return new WaitForSeconds(strikeEffectDuration);

        isUsingSkill = false;

        Debug.Log("Thunder Boss: Lightning Strike Finished!");
    }

    // =====================================
    // 4. ELECTRIC FIELD
    // =====================================

    private IEnumerator ElectricField()
    {
        isUsingSkill = true;

        Debug.Log("Thunder Boss: Electric Field!");

        if (player == null || electricFieldPrefab == null)
        {
            isUsingSkill = false;
            yield break;
        }

        // Tạo vùng điện tại vị trí Player
        Vector3 fieldPosition = player.transform.position;

        activeElectricField = Instantiate(
            electricFieldPrefab,
            fieldPosition,
            Quaternion.identity
        );

        activeElectricField.transform.localScale =
            Vector3.one * electricRadius * 2f;

        float elapsedTime = 0f;

        while (elapsedTime < electricDuration)
        {
            if (isDead)
            {
                ClearElectricField();
                yield break;
            }

            if (player != null)
            {
                float distance = Vector2.Distance(
                    fieldPosition,
                    player.transform.position
                );

                if (distance <= electricRadius)
                {
                    float damage = electricDamage *
                        (isRageMode ? rageDamageMultiplier : 1f);

                    player.TakeDamage(damage);
                }
            }

            yield return new WaitForSeconds(electricTickRate);

            elapsedTime += electricTickRate;
        }

        ClearElectricField();

        isUsingSkill = false;

        Debug.Log("Thunder Boss: Electric Field Finished!");
    }

    // =====================================
    // CLEAR EFFECTS
    // =====================================

    private void ClearWarning()
    {
        if (activeWarning != null)
        {
            Destroy(activeWarning);
            activeWarning = null;
        }
    }

    private void ClearElectricField()
    {
        if (activeElectricField != null)
        {
            Destroy(activeElectricField);
            activeElectricField = null;
        }
    }

    private void ClearAllEffects()
    {
        ClearWarning();
        ClearElectricField();

        if (activeStrikeEffect != null)
        {
            Destroy(activeStrikeEffect);
            activeStrikeEffect = null;
        }

        if (activeChainEffect != null)
        {
            Destroy(activeChainEffect);
            activeChainEffect = null;
        }
    }

    // =====================================
    // BOSS DEATH
    // =====================================

    protected override void Die()
    {
        if (isDead)
            return;

        Debug.Log("THUNDER BOSS DEFEATED!");

        ClearAllEffects();

        StopAllCoroutines();

        // Gọi BossEnemy.Die:
        // - GameManager.BossDefeated()
        // - Rơi USB
        // - Xóa Boss
        base.Die();
    }

    private void OnDestroy()
    {
        ClearAllEffects();
    }
}
