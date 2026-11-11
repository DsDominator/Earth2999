using UnityEngine;
using System.Collections;
public class EnemySpawner : MonoBehaviour
{
    [SerializeField] private GameObject[] enemies;
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private float timeBetweenSpawns = 2f;
    [SerializeField] private float minSpawnTime = 1f;
    [SerializeField] private float reduceSpawnTime = 0.2f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        StartCoroutine(SpawnEnemyCoroutine());
    }

    private IEnumerator SpawnEnemyCoroutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(timeBetweenSpawns);
            GameObject enemy = enemies[Random.Range(0,enemies.Length)];
            Transform spawnPoint = spawnPoints[Random.Range(0,spawnPoints.Length)];
            Instantiate(enemy,spawnPoint.position,Quaternion.identity);
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
