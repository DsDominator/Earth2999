
using UnityEngine;
using System.Collections;

[System.Serializable]
public class EnemySpawnEntry
{
    public GameObject enemyPrefab;

    [Min(0)]
    public float spawnWeight = 1f;
}

[System.Serializable]
public class PhaseSpawnSettings
{
    [Header("Phase")]
    public int phaseNumber = 1;

    [Header("Enemy Pool")]
    public EnemySpawnEntry[] enemies;
}

public class EnemySpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private float timeBetweenSpawns = 2f;

    [Header("Phase Settings")]
    [SerializeField] private PhaseSpawnSettings[] phaseSettings;

    private int currentPhase = 1;

    // =====================================
    // SET PHASE
    // =====================================

    public void SetPhase(int phaseNumber)
    {
        currentPhase = phaseNumber;

        Debug.Log("EnemySpawner: Phase " + currentPhase);
    }

    // =====================================
    // GET PHASE SETTINGS
    // =====================================

    private PhaseSpawnSettings GetCurrentPhaseSettings()
    {
        foreach (PhaseSpawnSettings settings in phaseSettings)
        {
            if (settings.phaseNumber == currentPhase)
                return settings;
        }

        Debug.LogWarning(
            "Không tìm thấy Enemy Pool cho Phase " + currentPhase
        );

        return null;
    }

    // =====================================
    // SPAWN WAVE
    // =====================================

    public IEnumerator SpawnWave(int enemyCount)
    {
        PhaseSpawnSettings settings = GetCurrentPhaseSettings();

        if (settings == null ||
            settings.enemies == null ||
            settings.enemies.Length == 0)
        {
            Debug.LogError("Enemy Pool của Phase hiện tại không hợp lệ!");
            yield break;
        }

        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogError("Chưa gán Spawn Points!");
            yield break;
        }

        Debug.Log(
            "Spawning " + enemyCount +
            " enemies in Phase " + currentPhase
        );

        for (int i = 0; i < enemyCount; i++)
        {
            GameObject selectedEnemy = GetRandomEnemy(settings);

            if (selectedEnemy != null)
            {
                Transform spawnPoint = spawnPoints[
                    Random.Range(0, spawnPoints.Length)
                ];

                Instantiate(
                    selectedEnemy,
                    spawnPoint.position,
                    Quaternion.identity
                );
            }

            if (i < enemyCount - 1)
                yield return new WaitForSeconds(timeBetweenSpawns);
        }

        Debug.Log("Spawn Wave Completed!");
    }

    // =====================================
    // RANDOM ENEMY BY WEIGHT
    // =====================================

    private GameObject GetRandomEnemy(PhaseSpawnSettings settings)
    {
        float totalWeight = 0f;

        foreach (EnemySpawnEntry entry in settings.enemies)
        {
            if (entry.enemyPrefab != null && entry.spawnWeight > 0)
                totalWeight += entry.spawnWeight;
        }

        if (totalWeight <= 0)
            return null;

        float randomValue = Random.Range(0f, totalWeight);

        foreach (EnemySpawnEntry entry in settings.enemies)
        {
            if (entry.enemyPrefab == null || entry.spawnWeight <= 0)
                continue;

            randomValue -= entry.spawnWeight;

            if (randomValue <= 0)
                return entry.enemyPrefab;
        }

        return null;
    }

    // =====================================
    // REDUCE SPAWN TIME
    // =====================================

    public void reduceTimeSpawms()
    {
        if (timeBetweenSpawns >= 1f)
        {
            timeBetweenSpawns -= 0.2f;
        }

        timeBetweenSpawns = Mathf.Max(0.3f, timeBetweenSpawns);

        Debug.Log("Spawn Time: " + timeBetweenSpawns);
    }
}
