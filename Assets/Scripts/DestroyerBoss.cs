
using UnityEngine;
using System.Collections;

public class DestroyerBoss : BossEnemy
{
    [Header("Laser Skill")]
    [SerializeField] private GameObject laserPrefab;
    [SerializeField] private float laserChargeTime = 1.5f;
    [SerializeField] private float laserDuration = 1f;
    [SerializeField] private float laserDamage = 25f;

    [Header("Laser Settings")]
    [SerializeField] private float laserScaleX = 1f;

    [Header("Mine Field")]
    [SerializeField] private GameObject minePrefab;
    [SerializeField] private int mineCount = 6;
    [SerializeField] private float mineRadius = 3f;

    [Header("Ground Slam")]
    [SerializeField] private float slamRadius = 4f;
    [SerializeField] private float slamDamage = 30f;
    [SerializeField] private float slamWarningTime = 1.5f;

    [SerializeField] private GameObject groundSlamWarningPrefab;
    [SerializeField] private GameObject groundSlamImpactPrefab;
    [SerializeField] private float slamEffectDuration = 1f;

    [Header("Turret")]
    [SerializeField] private GameObject turretPrefab;
    [SerializeField] private int turretCount = 2;

    [Header("Rage Mode")]
    [SerializeField] private float rageHealthPercent = 0.3f;
    [SerializeField] private float rageCooldownMultiplier = 0.5f;

    private bool isUsingSkill;
    private bool isRageMode;

    protected override void Update()
    {
        base.Update();

        if (isDead || isUsingSkill)
            return;

        CheckRageMode();
    }

    // =========================
    // RAGE MODE
    // =========================

    private void CheckRageMode()
    {
        if (isRageMode || maxHp <= 0)
            return;

        if (currentHp <= maxHp * rageHealthPercent)
        {
            isRageMode = true;

            skillCooldown *= rageCooldownMultiplier;
            enemyMovespeed *= 1.25f;

            Debug.Log("DESTROYER ENTERED RAGE MODE!");
        }
    }

    // =========================
    // RANDOM SKILL
    // =========================

    protected override void UseSkill()
    {
        if (isUsingSkill || isDead)
            return;

        nextSkillTime = Time.time + skillCooldown;

        int skill = Random.Range(0, 4);

        switch (skill)
        {
            case 0:
                StartCoroutine(LaserBeam());
                break;

            case 1:
                StartCoroutine(MineField());
                break;

            case 2:
                StartCoroutine(GroundSlam());
                break;

            case 3:
                SummonTurrets();
                break;
        }
    }

    // =========================
    // LASER BEAM
    // =========================


    private IEnumerator LaserBeam()
    {
        isUsingSkill = true;

        Debug.Log("Destroyer: Laser Charging!");

        if (laserPrefab == null || player == null)
        {
            isUsingSkill = false;
            yield break;
        }

        // Khóa hướng bắn ngay khi bắt đầu niệm
        Vector2 lockedDirection =
            (player.transform.position - transform.position).normalized;

        // Tạo Laser tại vị trí Boss
        GameObject laser = Instantiate(
            laserPrefab,
            transform.position,
            Quaternion.identity
        );

        laser.transform.right = lockedDirection;

        // Tìm các hiệu ứng con
        Transform charge = laser.transform.Find("LaserCharge");
        Transform core = laser.transform.Find("LaserCore");
        Transform beam = laser.transform.Find("LaserBeam");

        if (charge == null || core == null || beam == null)
        {
            Debug.LogError("Laser prefab is missing child objects!");

            Destroy(laser);
            isUsingSkill = false;
            yield break;
        }

        // Giai đoạn cảnh báo
        charge.gameObject.SetActive(true);
        core.gameObject.SetActive(true);
        beam.gameObject.SetActive(false);

        // Boss tích tụ năng lượng, Laser đi theo vị trí Boss
        float elapsedTime = 0f;

        while (elapsedTime < laserChargeTime)
        {
            if (isDead || player == null)
            {
                Destroy(laser);
                isUsingSkill = false;
                yield break;
            }

            laser.transform.position = transform.position;

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Kiểm tra trước khi bắn
        if (isDead || player == null)
        {
            Destroy(laser);
            isUsingSkill = false;
            yield break;
        }

        // Giữ nguyên hướng đã khóa
        laser.transform.right = lockedDirection;

        // Tắt hiệu ứng tích tụ
        charge.gameObject.SetActive(false);
        core.gameObject.SetActive(false);

        // Bật Laser
        beam.gameObject.SetActive(true);

        // Điều chỉnh chiều dài Laser
        Vector3 beamScale = beam.localScale;
        beamScale.x = laserScaleX;
        beam.localScale = beamScale;

        // Cài đặt sát thương
        BossSkillDamage damage =
            beam.GetComponent<BossSkillDamage>();

        if (damage == null)
        {
            damage = beam.GetComponentInChildren<BossSkillDamage>();
        }

        if (damage != null)
        {
            damage.SetDamage(laserDamage);
        }
        else
        {
            Debug.LogWarning("BossSkillDamage not found on LaserBeam!");
        }

        Debug.Log("Destroyer: Laser Fired!");

        // Duy trì Laser
        yield return new WaitForSeconds(laserDuration);

        // Kết thúc kỹ năng
        Destroy(laser);

        isUsingSkill = false;

        Debug.Log("Destroyer: Laser Finished!");
    }


    // =========================
    // MINE FIELD
    // =========================

    private IEnumerator MineField()
    {
        isUsingSkill = true;

        Debug.Log("Destroyer: Mine Field!");

        for (int i = 0; i < mineCount; i++)
        {
            if (minePrefab == null)
                break;

            Vector2 randomPosition =
                Random.insideUnitCircle * mineRadius;

            Instantiate(
                minePrefab,
                (Vector2)transform.position + randomPosition,
                Quaternion.identity
            );
        }

        yield return new WaitForSeconds(1f);

        isUsingSkill = false;
    }

    // =========================
    // GROUND SLAM
    // =========================

    
    
    private IEnumerator GroundSlam()
    {
        isUsingSkill = true;

        Debug.Log("Destroyer: Ground Slam Charging!");

        // Lưu vị trí Boss tại thời điểm bắt đầu kỹ năng
        Vector3 slamPosition = transform.position;

        // Kích thước vùng tấn công
        float slamSize = slamRadius * 2f;

        // =========================
        // GIAI ĐOẠN 1: CẢNH BÁO
        // =========================

        GameObject warning = null;

        if (groundSlamWarningPrefab != null)
        {
            warning = Instantiate(
                groundSlamWarningPrefab,
                slamPosition,
                Quaternion.identity
            );

            warning.transform.localScale =
                Vector3.one * slamSize;
        }

        // Chờ Player né tránh
        yield return new WaitForSeconds(slamWarningTime);

        if (isDead)
        {
            if (warning != null)
                Destroy(warning);

            isUsingSkill = false;
            yield break;
        }

        // Xóa hiệu ứng cảnh báo
        if (warning != null)
            Destroy(warning);

        // =========================
        // GIAI ĐOẠN 2: VA CHẠM
        // =========================

        Debug.Log("Destroyer: Ground Slam Impact!");

        if (groundSlamImpactPrefab != null)
        {
            GameObject impact = Instantiate(
                groundSlamImpactPrefab,
                slamPosition,
                Quaternion.identity
            );

            impact.transform.localScale =
                Vector3.one * slamSize;

            Destroy(impact, slamEffectDuration);
        }

        // =========================
        // GIAI ĐOẠN 3: GÂY SÁT THƯƠNG
        // =========================

        if (player != null)
        {
            float distance = Vector2.Distance(
                slamPosition,
                player.transform.position
            );

            if (distance <= slamRadius)
            {
                Player target = player.GetComponent<Player>();

                if (target == null)
                    target = player.GetComponentInParent<Player>();

                if (target != null)
                {
                    target.TakeDamage(slamDamage);

                    Debug.Log(
                        "Ground Slam hit Player! Damage: " + slamDamage
                    );
                }
            }
        }

        // Chờ hiệu ứng kết thúc
        yield return new WaitForSeconds(slamEffectDuration);

        isUsingSkill = false;

        Debug.Log("Destroyer: Ground Slam Finished!");
    }



    // =========================
    // SUMMON TURRETS
    // =========================

    private void SummonTurrets()
    {
        isUsingSkill = true;

        Debug.Log("Destroyer: Summon Turrets!");

        if (turretPrefab != null)
        {
            for (int i = 0; i < turretCount; i++)
            {
                Vector2 position =
                    (Vector2)transform.position +
                    Random.insideUnitCircle * 3f;

                Instantiate(
                    turretPrefab,
                    position,
                    Quaternion.identity
                );
            }
        }

        isUsingSkill = false;
    }
}
