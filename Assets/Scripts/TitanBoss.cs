using System.Collections;
using UnityEngine;

/// <summary>Close-range Titan boss with area attacks, a charge, and a timed armor phase.</summary>
public class TitanBoss : BossEnemy
{
    [Header("Titan Skill Settings")]
    [SerializeField] private float warningTime = 0.8f;
    [SerializeField] private GameObject slamWarningPrefab;
    [SerializeField] private GameObject slamImpactPrefab;
    [SerializeField] private GameObject shockwavePrefab;
    [SerializeField] private float slamImpactDuration = 1f;

    [Header("Iron Slam")]
    [SerializeField] private float slamDamage = 30f;
    [SerializeField] private float slamRadius = 3.5f;

    [Header("Steel Charge")]
    [SerializeField] private float chargeDamage = 25f;
    [SerializeField] private float chargeSpeed = 12f;
    [SerializeField] private float chargeDuration = 0.7f;
    [SerializeField] private float chargeHitRadius = 1f;

    [Header("Ground Shockwave")]
    [SerializeField] private float shockwaveDamage = 20f;
    [SerializeField] private float shockwaveRadius = 7f;
    [SerializeField] private float shockwaveDuration = 0.8f;

    [Header("Iron Fortress")]
    [SerializeField] private float fortressDuration = 4f;
    [SerializeField] private Color fortressColor = new Color(1f, 0.65f, 0.2f, 1f);
    [SerializeField] private float fortressPulseSpeed = 5f;

    [Header("Overload")]
    [Range(0.01f, 0.9f)]
    [SerializeField] private float overloadHealthPercent = 0.3f;
    [SerializeField] private float overloadSpeedMultiplier = 1.5f;
    [SerializeField] private float overloadDamageMultiplier = 1.5f;

    private bool isFortified;
    private bool isOverloaded;
    private float baseMoveSpeed;
    private GameObject activeWarning;
    private SpriteRenderer visualRenderer;

    protected override void Start()
    {
        base.Start();
        baseMoveSpeed = enemyMovespeed;
        visualRenderer = GetComponentInChildren<SpriteRenderer>();
    }

    protected override void Update()
    {
        base.Update();

        if (isDead)
            return;

        if (!isOverloaded && maxHp > 0f && currentHp <= maxHp * overloadHealthPercent)
        {
            isOverloaded = true;
            enemyMovespeed = baseMoveSpeed * overloadSpeedMultiplier;
        }
    }

    protected override void UseSkill()
    {
        if (isDead || player == null)
            return;

        nextSkillTime = Time.time + skillCooldown;

        switch (Random.Range(0, 4))
        {
            case 0:
                PlaySkillAnimation();
                StartCoroutine(IronSlam());
                break;
            case 1:
                StartCoroutine(SteelCharge());
                break;
            case 2:
                PlaySkillAnimation();
                StartCoroutine(GroundShockwave());
                break;
            default:
                StartCoroutine(IronFortress());
                break;
        }
    }

    private IEnumerator IronSlam()
    {
        Vector2 impactPoint = player != null ? (Vector2)player.transform.position : (Vector2)transform.position;
        activeWarning = SpawnEffect(slamWarningPrefab, impactPoint, slamRadius * 2f);
        yield return new WaitForSeconds(warningTime);

        ClearWarning();
        if (isDead)
            yield break;

        GameObject impact = SpawnEffect(slamImpactPrefab, impactPoint, slamRadius * 2f);
        if (impact != null)
            Destroy(impact, slamImpactDuration);
        DamagePlayerInRadius(impactPoint, slamRadius, GetAttackDamage(slamDamage));
    }

    private IEnumerator SteelCharge()
    {
        if (player == null)
            yield break;

        Vector2 direction = ((Vector2)player.transform.position - (Vector2)transform.position).normalized;
        if (direction.sqrMagnitude < 0.001f)
            direction = Vector2.right;

        float elapsed = 0f;
        bool hitPlayer = false;
        Transform visual = visualRenderer != null ? visualRenderer.transform : null;
        Vector3 originalScale = visual != null ? visual.localScale : Vector3.one;

        try
        {
            while (elapsed < chargeDuration && !isDead)
            {
                float progress = Mathf.Clamp01(elapsed / chargeDuration);
                float stretch = Mathf.Sin(progress * Mathf.PI);
                if (visual != null)
                    visual.localScale = Vector3.Scale(originalScale, new Vector3(1f + stretch * 0.35f, 1f - stretch * 0.2f, 1f));

                float step = Mathf.Min(chargeSpeed * Time.deltaTime, chargeSpeed * (chargeDuration - elapsed));
                transform.position += (Vector3)(direction * step);
                elapsed += Time.deltaTime;

                if (!hitPlayer)
                {
                    Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, chargeHitRadius);
                    foreach (Collider2D hit in hits)
                    {
                        Player target = hit.GetComponentInParent<Player>();
                        if (target == null)
                            continue;
                        target.TakeDamage(GetAttackDamage(chargeDamage));
                        hitPlayer = true;
                        break;
                    }
                }

                yield return null;
            }
        }
        finally
        {
            if (visual != null)
                visual.localScale = originalScale;
        }
    }

    private IEnumerator GroundShockwave()
    {
        Vector2 origin = transform.position;
        GameObject wave = SpawnEffect(shockwavePrefab, origin, shockwaveRadius * 2f);
        float elapsed = 0f;
        float previousRadius = 0f;
        bool hitPlayer = false;

        while (elapsed < shockwaveDuration && !isDead)
        {
            elapsed += Time.deltaTime;
            float currentRadius = shockwaveRadius * Mathf.Clamp01(elapsed / shockwaveDuration);
            if (!hitPlayer && player != null)
            {
                float distance = Vector2.Distance(origin, player.transform.position);
                if (distance >= previousRadius && distance <= currentRadius + 0.15f)
                {
                    player.TakeDamage(GetAttackDamage(shockwaveDamage));
                    hitPlayer = true;
                }
            }

            if (wave != null)
                wave.transform.localScale = Vector3.one * Mathf.Max(0.1f, currentRadius * 2f);
            previousRadius = currentRadius;
            yield return null;
        }

        if (wave != null)
            Destroy(wave);
    }

    private IEnumerator IronFortress()
    {
        isFortified = true;
        Color originalColor = visualRenderer != null ? visualRenderer.color : Color.white;
        float elapsed = 0f;

        try
        {
            while (elapsed < fortressDuration && !isDead)
            {
                float pulse = (Mathf.Sin(Time.time * fortressPulseSpeed) + 1f) * 0.5f;
                if (visualRenderer != null)
                    visualRenderer.color = Color.Lerp(originalColor, fortressColor, 0.35f + pulse * 0.45f);

                elapsed += Time.deltaTime;
                yield return null;
            }
        }
        finally
        {
            isFortified = false;
            if (visualRenderer != null)
                visualRenderer.color = originalColor;
        }
    }

    public override void TakeDamage(float damage)
    {
        if (!isFortified)
            base.TakeDamage(damage);
    }

    public override void TakeDamage(float damage, bool isCritical)
    {
        if (!isFortified || isCritical)
            base.TakeDamage(damage);
    }

    private float GetAttackDamage(float damage)
    {
        return isOverloaded ? damage * overloadDamageMultiplier : damage;
    }

    private void DamagePlayerInRadius(Vector2 center, float radius, float damage)
    {
        if (player != null && Vector2.Distance(center, player.transform.position) <= radius)
            player.TakeDamage(damage);
    }

    private GameObject SpawnEffect(GameObject prefab, Vector2 position, float diameter)
    {
        if (prefab == null)
            return null;

        GameObject effect = Instantiate(prefab, position, Quaternion.identity);
        effect.transform.localScale = Vector3.one * diameter;
        return effect;
    }

    private void ClearWarning()
    {
        if (activeWarning != null)
        {
            Destroy(activeWarning);
            activeWarning = null;
        }
    }

    protected override void Die()
    {
        ClearWarning();
        StopAllCoroutines();
        base.Die();
    }

    private void OnDestroy()
    {
        ClearWarning();
    }
}
