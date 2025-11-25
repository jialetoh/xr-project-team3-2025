// Adapted from: https://github.com/llamacademy/ai-series-part-4/blob/main/Assets/Scripts/EnemySpawner.cs

using System.Collections;
using System.Collections.Generic;
using UnityEditor.EditorTools;
using UnityEngine;
using UnityEngine.AI;

public class EnemySpawner : MonoBehaviour
{
    [Header("Enemy Spawning Settings")]
    [Tooltip("The player transform for enemies to chase.")]
    public Transform Player;
    [Tooltip("Number of enemies to spawn.")]
    public int NumberOfEnemiesToSpawn = 5;
    [Tooltip("Delay between enemy spawns.")]
    public float SpawnDelay = 1f;
    [Tooltip("Should enemies be spawned continuously?")]
    public bool ContinuousSpawning;

    [Header("Enemy Types")]
    [Tooltip("List of enemy prefabs to spawn.")]
    public List<EnemyScriptableObject> Enemies = new();
    [Tooltip("Method to use for spawning enemies.")]
    public SpawnMethod EnemySpawnMethod = SpawnMethod.RoundRobin;

    [Header("Difficulty Settings")]
    [Tooltip("The current level of enemy difficulty.")]
    [SerializeField]
    private int Level = 0;
    [Tooltip("Tracks the number of enemies spawned so far.")]
    private int InitialEnemiesToSpawn;
    [Tooltip("Tracks the spawn delay at start.")]
    private float InitialSpawnDelay;

    [Tooltip("Tracks the number of enemies spawned so far.")]
    private int SpawnedEnemies = 0;
    [Tooltip("Tracks the number of enemies currently alive.")]
    private int EnemiesAlive = 0;

    private NavMeshTriangulation Triangulation;
    private Dictionary<int, ObjectPool> EnemyObjectPools = new();

    private void Awake()
    {
        for (int i = 0; i < Enemies.Count; i++)
        {
            EnemyObjectPools.Add(i, ObjectPool.CreateInstance(Enemies[i].Prefab, NumberOfEnemiesToSpawn));
        }

        InitialEnemiesToSpawn = NumberOfEnemiesToSpawn;
        InitialSpawnDelay = SpawnDelay;
    }

    private void Start()
    {
        Triangulation = NavMesh.CalculateTriangulation();

        StartCoroutine(SpawnEnemies());
    }

    private IEnumerator SpawnEnemies()
    {
        Level++;
        SpawnedEnemies = 0;
        EnemiesAlive = 0;

        WaitForSeconds Wait = new(SpawnDelay);

        while (SpawnedEnemies < NumberOfEnemiesToSpawn)
        {
            if (EnemySpawnMethod == SpawnMethod.RoundRobin)
            {
                SpawnRoundRobinEnemy(SpawnedEnemies);
            }
            else if (EnemySpawnMethod == SpawnMethod.Random)
            {
                SpawnRandomEnemy();
            }

            SpawnedEnemies++;

            yield return Wait;
        }

        if (ContinuousSpawning)
        {
            StartCoroutine(SpawnEnemies());
        }
    }

    private void SpawnRoundRobinEnemy(int SpawnedEnemies)
    {
        int SpawnIndex = SpawnedEnemies % Enemies.Count;

        DoSpawnEnemy(SpawnIndex, ChooseRandomPositionOnNavMesh());
    }

    private void SpawnRandomEnemy()
    {
        DoSpawnEnemy(Random.Range(0, Enemies.Count), ChooseRandomPositionOnNavMesh());
    }

    private Vector3 ChooseRandomPositionOnNavMesh()
    {
        int VertexIndex = Random.Range(0, Triangulation.vertices.Length);
        return Triangulation.vertices[VertexIndex];
    }

    public void DoSpawnEnemy(int SpawnIndex, Vector3 SpawnPosition)
    {
        PoolableObject poolableObject = EnemyObjectPools[SpawnIndex].GetObject();

        if (poolableObject == null)
        {
            Debug.LogError($"Unable to fetch enemy of type {SpawnIndex} from object pool. Out of objects?");
            return;
        }

        Enemy enemy = poolableObject.GetComponent<Enemy>();
        Enemies[SpawnIndex].SetUpEnemy(enemy);

        if (NavMesh.SamplePosition(SpawnPosition, out NavMeshHit Hit, 2f, -1))
        {
            enemy.Agent.Warp(Hit.position);
            // enemy needs to get enabled and start chasing now.
            enemy.Movement.Player = Player;
            enemy.Movement.Triangulation = Triangulation;
            enemy.Agent.enabled = true;
            // enemy.Movement.Spawn();
            enemy.Movement.StartChasing();
            enemy.OnDeath += OnEnemyDied;

            EnemiesAlive++;
        }
        else
        {
            Debug.LogError($"Unable to place NavMeshAgent on NavMesh. Tried to use {SpawnPosition}");
        }
    }

    private void OnEnemyDied(Enemy deadEnemy)
    {
        EnemiesAlive--;

        if (EnemiesAlive == 0 && SpawnedEnemies == NumberOfEnemiesToSpawn)
        {
            StartCoroutine(SpawnEnemies()); // Start a new wave
        }
    }

    public enum SpawnMethod
    {
        RoundRobin,
        Random
        // Other spawn methods can be added here
    }
}