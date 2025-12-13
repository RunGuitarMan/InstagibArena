using UnityEngine;
using System.Collections.Generic;

public class SpawnManager : MonoBehaviour
{
    public static SpawnManager Instance { get; private set; }

    [Header("Spawn Settings")]
    public int maxEnemies = 5;
    public float respawnDelay = 2f;
    public float minSpawnDistance = 15f; // Min distance from player

    [Header("Enemy Prefab Settings")]
    public float enemyHeight = 2f;
    public float enemyRadius = 0.5f;

    [Header("References")]
    public Transform player;
    public List<Transform> spawnPoints = new List<Transform>();

    private List<Enemy> activeEnemies = new List<Enemy>();
    private int enemiesKilledCount = 0;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player")?.transform;
        }

        // Initial spawn
        SpawnInitialEnemies();
    }

    void SpawnInitialEnemies()
    {
        for (int i = 0; i < maxEnemies; i++)
        {
            SpawnEnemy();
        }
    }

    public void SpawnEnemy()
    {
        if (activeEnemies.Count >= maxEnemies) return;

        Vector3 spawnPos = GetSpawnPosition();
        GameObject enemyObj = CreateEnemyObject(spawnPos);
        Enemy enemy = enemyObj.GetComponent<Enemy>();
        activeEnemies.Add(enemy);
    }

    GameObject CreateEnemyObject(Vector3 position)
    {
        // Create enemy from primitives
        GameObject enemy = new GameObject("Enemy");
        enemy.transform.position = position;
        enemy.tag = "Enemy";
        enemy.layer = LayerMask.NameToLayer("Default");

        // Body (capsule)
        GameObject body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        body.transform.SetParent(enemy.transform);
        body.transform.localPosition = Vector3.up * 1f;
        body.transform.localScale = new Vector3(enemyRadius * 2, 1f, enemyRadius * 2);

        // Head indicator (small sphere)
        GameObject head = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        head.transform.SetParent(enemy.transform);
        head.transform.localPosition = Vector3.up * 2f;
        head.transform.localScale = Vector3.one * 0.4f;

        // CharacterController for movement
        CharacterController cc = enemy.AddComponent<CharacterController>();
        cc.height = enemyHeight;
        cc.radius = enemyRadius;
        cc.center = Vector3.up * 1f;

        // Enemy script
        enemy.AddComponent<Enemy>();

        return enemy;
    }

    Vector3 GetSpawnPosition()
    {
        // Try to find a spawn point far from player
        if (spawnPoints.Count > 0)
        {
            List<Transform> validPoints = new List<Transform>();

            foreach (Transform point in spawnPoints)
            {
                if (player == null || Vector3.Distance(point.position, player.position) >= minSpawnDistance)
                {
                    validPoints.Add(point);
                }
            }

            if (validPoints.Count > 0)
            {
                return validPoints[Random.Range(0, validPoints.Count)].position;
            }
            else
            {
                return spawnPoints[Random.Range(0, spawnPoints.Count)].position;
            }
        }

        // Fallback - random position on arena
        Vector3 randomPos = new Vector3(
            Random.Range(-20f, 20f),
            1f,
            Random.Range(-20f, 20f)
        );

        // Make sure it's far enough from player
        if (player != null)
        {
            int attempts = 0;
            while (Vector3.Distance(randomPos, player.position) < minSpawnDistance && attempts < 10)
            {
                randomPos = new Vector3(
                    Random.Range(-20f, 20f),
                    1f,
                    Random.Range(-20f, 20f)
                );
                attempts++;
            }
        }

        return randomPos;
    }

    public void OnEnemyDied(Enemy enemy)
    {
        activeEnemies.Remove(enemy);
        enemiesKilledCount++;

        // Respawn after delay
        Invoke(nameof(SpawnEnemy), respawnDelay);
    }

    public Vector3 GetPlayerSpawnPosition()
    {
        // Find spawn point furthest from any enemy
        if (spawnPoints.Count > 0)
        {
            Transform bestPoint = spawnPoints[0];
            float bestDistance = 0f;

            foreach (Transform point in spawnPoints)
            {
                float minDistToEnemy = float.MaxValue;

                foreach (Enemy enemy in activeEnemies)
                {
                    if (enemy != null)
                    {
                        float dist = Vector3.Distance(point.position, enemy.transform.position);
                        if (dist < minDistToEnemy)
                            minDistToEnemy = dist;
                    }
                }

                if (minDistToEnemy > bestDistance)
                {
                    bestDistance = minDistToEnemy;
                    bestPoint = point;
                }
            }

            return bestPoint.position;
        }

        // Fallback
        return new Vector3(0, 1f, 0);
    }

    public void AddSpawnPoint(Vector3 position)
    {
        GameObject spawnPoint = new GameObject("SpawnPoint");
        spawnPoint.transform.position = position;
        spawnPoints.Add(spawnPoint.transform);
    }
}
