using UnityEngine;
using System.Collections;
public class EnemySpawner : MonoBehaviour
{
    [SerializeField] private GameObject[] enemies;
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private float timeBetweenSpawns = 2f;
    [SerializeField] private float minSpawnTime = 1f;
    [SerializeField] private float reduceSpawnTime = 0.2f;

    public IEnumerator SpawnWave(int enemyCount)
    {
        for (int i = 0; i < enemyCount; i++)
        {
            GameObject enemy = enemies[Random.Range(0, enemies.Length)];
            Transform spawnPoint = spawnPoints[
                Random.Range(0, spawnPoints.Length)
            ];

            Instantiate(enemy, spawnPoint.position, Quaternion.identity);

            yield return new WaitForSeconds(timeBetweenSpawns);
        }
    }
    public void reduceTimeSpawms()
    {
        timeBetweenSpawns = Mathf.Max(
            minSpawnTime,
            timeBetweenSpawns - reduceSpawnTime
        );

        Debug.Log("New Spawn Time: " + timeBetweenSpawns);
    }

}
