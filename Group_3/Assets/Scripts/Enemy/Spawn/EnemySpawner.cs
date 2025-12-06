// Adapted from: https://github.com/llamacademy/ai-series-part-4/blob/main/Assets/Scripts/EnemySpawner.cs

using System.Collections;
using System.Collections.Generic;
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
    public List<WeightedSpawnScriptableObject> WeightedEnemies = new();
    [Tooltip("Method to use for spawning enemies.")]
    public SpawnMethod EnemySpawnMethod = SpawnMethod.RoundRobin;
    [Tooltip("Weights for each enemy type when using weighted random spawning.")]
    [SerializeField]
    private float[] Weights;

    [Space]
    [Header("Difficulty Settings")]
    [Tooltip("Scaling configuration for enemy difficulty.")]
    public ScalingScriptableObject Scaling;
    [Tooltip("The current level of enemy difficulty.")]
    [SerializeField]
    private int Level = 0;
    [Tooltip("Tracks the number of enemies spawned so far.")]
    private int InitialEnemiesToSpawn;
    [Tooltip("Tracks the spawn delay at start.")]
    private float InitialSpawnDelay;
    [Tooltip("List of scaled enemy configurations for the current level.")]
    [SerializeField]
    private List<EnemyScriptableObject> ScaledEnemies = new();

    [Tooltip("Tracks the number of enemies spawned so far.")]
    private int SpawnedEnemies = 0;
    [Tooltip("Tracks the number of enemies currently alive.")]
    private int EnemiesAlive = 0;

    private NavMeshTriangulation Triangulation;
    private readonly Dictionary<int, ObjectPool> EnemyObjectPools = new();

    private void Awake()
    {
        for (int i = 0; i < WeightedEnemies.Count; i++)
        {
            EnemyObjectPools.Add(i, ObjectPool.CreateInstance(WeightedEnemies[i].Enemy.Prefab, NumberOfEnemiesToSpawn));
        }

        InitialEnemiesToSpawn = NumberOfEnemiesToSpawn;
        InitialSpawnDelay = SpawnDelay;

        Weights = new float[WeightedEnemies.Count];
    }

    private void Start()
    {
        Triangulation = NavMesh.CalculateTriangulation();

        for (int i = 0; i < WeightedEnemies.Count; i++)
        {
            ScaledEnemies.Add(WeightedEnemies[i].Enemy.ScaleUpForLevel(Scaling, 0));
        }

        StartCoroutine(SpawnEnemies());
    }

    private IEnumerator SpawnEnemies()
    {
        // Increase difficulty for next wave
        Level++;
        SpawnedEnemies = 0;
        EnemiesAlive = 0;
        for (int i = 0; i < WeightedEnemies.Count; i++)
        {
            ScaledEnemies[i] = WeightedEnemies[i].Enemy.ScaleUpForLevel(Scaling, Level);
        }

        ResetSpawnWeights();

        // Start spawning enemies
        WaitForSeconds Wait = new(SpawnDelay);
        while (SpawnedEnemies < NumberOfEnemiesToSpawn)
        {
            switch (EnemySpawnMethod)
            {
                case SpawnMethod.RoundRobin:
                    SpawnRoundRobinEnemy(SpawnedEnemies);
                    break;
                case SpawnMethod.Random:
                    SpawnRandomEnemy();
                    break;
                case SpawnMethod.WeightedRandom:
                    SpawnWeightedRandomEnemy();
                    break;
                default:
                    Debug.LogError("Unknown enemy spawn method.");
                    break;
            }

            SpawnedEnemies++;
            yield return Wait;
        }

        // If continuous spawning is enabled, start the next wave
        if (ContinuousSpawning)
        {
            ScaleUpSpawns();
            StartCoroutine(SpawnEnemies());
        }
    }

    private void SpawnRoundRobinEnemy(int SpawnedEnemies)
    {
        int SpawnIndex = SpawnedEnemies % WeightedEnemies.Count;

        DoSpawnEnemy(SpawnIndex, ChooseRandomPositionOnNavMesh());
    }

    private void SpawnRandomEnemy()
    {
        DoSpawnEnemy(Random.Range(0, WeightedEnemies.Count), ChooseRandomPositionOnNavMesh());
    }

    private void SpawnWeightedRandomEnemy()
    {
        float Value = Random.value;

        for (int i = 0; i < Weights.Length; i++)
        {
            if (Value < Weights[i])
            {
                DoSpawnEnemy(i, ChooseRandomPositionOnNavMesh());
                return;
            }

            Value -= Weights[i];
        }

        Debug.LogError("Invalid configuration! Could not spawn a Weighted Random Enemy. Did you forget to call ResetSpawnWeights()?");
    }

    /// <summary>
    /// Resets the spawn weights for weighted random spawning.
    /// </summary>
    private void ResetSpawnWeights()
    {
        float TotalWeight = 0;

        for (int i = 0; i < WeightedEnemies.Count; i++)
        {
            Weights[i] = WeightedEnemies[i].GetWeight();
            TotalWeight += Weights[i];
        }

        for (int i = 0; i < Weights.Length; i++)
        {
            Weights[i] = Weights[i] / TotalWeight;
        }
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
        ScaledEnemies[SpawnIndex].SetUpEnemy(enemy);

        if (NavMesh.SamplePosition(SpawnPosition, out NavMeshHit Hit, 2f, -1))
        {
            enemy.Agent.Warp(Hit.position);
            // enemy needs to get enabled and start chasing now.
            enemy.Movement.Player = Player;
            enemy.Movement.Triangulation = Triangulation;
            enemy.Agent.enabled = true;
            enemy.Movement.Spawn();
            enemy.OnDeath += OnEnemyDied;

            EnemiesAlive++;
        }
        else
        {
            Debug.LogError($"Unable to place NavMeshAgent on NavMesh. Tried to use {SpawnPosition}");
        }
    }

    /// <summary>
    /// Scales up the spawn settings for the next wave.<br />
    /// The scaling is based on the provided Scaling configuration and current Level.
    /// </summary>
    private void ScaleUpSpawns()
    {
        NumberOfEnemiesToSpawn = Mathf.FloorToInt(InitialEnemiesToSpawn * Scaling.SpawnCountCurve.Evaluate(Level + 1));
        SpawnDelay = InitialSpawnDelay * Scaling.SpawnRateCurve.Evaluate(Level + 1);
    }

    /// <summary>
    /// Chooses a random position on the NavMesh for spawning an enemy.
    /// </summary>
    /// <returns>The chosen position on the NavMesh.</returns>
    private Vector3 ChooseRandomPositionOnNavMesh()
    {
        int VertexIndex = Random.Range(0, Triangulation.vertices.Length);
        return Triangulation.vertices[VertexIndex];
    }

    private void OnEnemyDied(Enemy deadEnemy)
    {
        EnemiesAlive--;

        if (EnemiesAlive == 0 && SpawnedEnemies == NumberOfEnemiesToSpawn)
        {
            StartCoroutine(SpawnEnemies()); // Start a new wave
        }
    }

    /// <summary>
    /// Methods for different enemy spawn strategies.
    /// </summary>
    public enum SpawnMethod
    {
        /// <summary>
        /// Spawns enemies in a round-robin fashion from the list.
        /// </summary>
        RoundRobin,

        /// <summary>
        /// Spawns enemies randomly.
        /// </summary>
        Random,

        /// <summary>
        /// Spawns enemies based on weighted random selection.
        /// </summary>
        WeightedRandom,
    }
}