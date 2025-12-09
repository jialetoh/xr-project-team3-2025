// Adapted from: https://github.com/llamacademy/ai-series-part-18/blob/master/Assets/Scripts/EnemyBurstSpawnArea.cs

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class EnemyBurstSpawner : MonoBehaviour
{
    [Tooltip("The collider defining the spawn area.")]
    [SerializeField]
    private Collider SpawnCollider;

    [Header("Enemy Burst Spawn Settings")]
    [Tooltip("The enemy spawner responsible for spawning enemies.")]
    [SerializeField]
    private EnemySpawner EnemySpawner;
    [Tooltip("List of enemy types to spawn.")]
    [SerializeField]
    private List<EnemyScriptableObject> Enemies = new();
    [Tooltip("The method used to spawn enemies.")]
    [SerializeField]
    private EnemySpawner.SpawnMethod SpawnMethod = EnemySpawner.SpawnMethod.Random;
    [Tooltip("Number of enemies to spawn in the burst.")]
    [SerializeField]
    private int SpawnCount = 10;
    [Tooltip("Delay between spawns within the burst.")]
    [SerializeField]
    private float SpawnDelay = 0.5f;

    [Tooltip("Coroutine handling the enemy spawning.")]
    private Coroutine SpawnEnemiesCoroutine;
    [Tooltip("Bounds of the spawn area.")]
    private Bounds Bounds;

    private void Awake()
    {
        if (SpawnCollider == null)
        {
            SpawnCollider = GetComponent<Collider>();
        }

        Bounds = SpawnCollider.bounds;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (SpawnEnemiesCoroutine == null)
        {
            SpawnEnemiesCoroutine = StartCoroutine(SpawnEnemies());
        }
    }

    private IEnumerator SpawnEnemies()
    {
        WaitForSeconds Wait = new(SpawnDelay);

        for (int i = 0; i < SpawnCount; i++)
        {
            switch (SpawnMethod)
            {
                case EnemySpawner.SpawnMethod.RoundRobin:
                    EnemySpawner.DoSpawnEnemy(
                        EnemySpawner.WeightedEnemies.FindIndex((enemy) => enemy.Enemy.Equals(Enemies[i % Enemies.Count])),
                        GetRandomPositionInBounds()
                    );
                    break;
                case EnemySpawner.SpawnMethod.Random:
                    int index = Random.Range(0, Enemies.Count);
                    EnemySpawner.DoSpawnEnemy(
                        EnemySpawner.WeightedEnemies.FindIndex((enemy) => enemy.Enemy.Equals(Enemies[index])),
                        GetRandomPositionInBounds()
                    );
                    break;
            }

            yield return Wait;
        }

        Destroy(gameObject);
    }

    private Vector3 GetRandomPositionInBounds()
    {
        return new Vector3(
            Random.Range(Bounds.min.x, Bounds.max.x),
            Bounds.min.y,
            Random.Range(Bounds.min.z, Bounds.max.z)
        );
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;

        if (SpawnCollider != null)
        {
            Gizmos.DrawWireCube(SpawnCollider.bounds.center, SpawnCollider.bounds.size);
        }
        else
        {
            Collider collider = GetComponent<Collider>();
            Gizmos.DrawWireCube(collider.bounds.center, collider.bounds.size);
        }
    }
}