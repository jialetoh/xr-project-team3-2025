// Adapted from: https://github.com/llamacademy/ai-series-part-5/blob/main/Assets/Scripts/Enemy/EnemyScriptableObject.cs

using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// ScriptableObject that holds the BASE STATS for an enemy. These can then be modified at object creation time to buff up enemies
/// and to reset their stats if they died or were modified at runtime.
/// </summary>
[CreateAssetMenu(fileName = "Enemy Configuration", menuName = "ScriptableObject/Enemy Configuration")]
public class EnemyScriptableObject : ScriptableObject
{
    [Header("Prefab")]
    [Tooltip("The enemy prefab associated with this configuration.")]
    public Enemy Prefab;
    [Tooltip("The attack configuration associated with this enemy.")]
    public AttackScriptableObject AttackConfiguration;

    [Header("Enemy Stats")]
    [Tooltip("The base health of the enemy")]
    public int Health = 100;
    [Tooltip("The time delay between enemy attacks")]
    public float AttackDelay = 1f;
    [Tooltip("The damage dealt by the enemy per attack")]
    public int Damage = 5;
    [Tooltip("The radius within which the enemy can attack")]
    public float AttackRadius = 1.5f;
    [Tooltip("Does the enemy have a ranged attack?")]
    public bool HasRangedAttack = false;

    [Header("Enemy Behavior Settings")]
    [Tooltip("The default state of the enemy.")]
    public EnemyState DefaultState;
    [Tooltip("The radius within which the enemy can idle.")]
    public float IdleLocationRadius = 4f;
    [Tooltip("The movement speed multiplier when the enemy is idle.")]
    public float IdleMovespeedMultiplier = 0.5f;
    [Range(2, 10)]
    [Tooltip("The number of waypoints for patrolling.")]
    public int Waypoints = 4;
    [Tooltip("The range within which the enemy can see the player.")]
    public float LineOfSightRange = 6f;
    [Tooltip("The field of view angle of the enemy.")]
    public float FieldOfView = 90f;


    [Header("NavMesh Agent Settings")]
    [Tooltip("The interval at which the AI updates its pathfinding")]
    public float AIUpdateInterval = 0.1f;

    [Tooltip("The acceleration of the NavMesh agent")]
    public float Acceleration = 8;
    [Tooltip("The angular speed of the NavMesh agent")]
    public float AngularSpeed = 120;
    // -1 means everything
    [Tooltip("The area mask of the NavMesh agent")]
    public int AreaMask = -1;

    [Tooltip("The avoidance priority of the NavMesh agent")]
    public int AvoidancePriority = 50;

    [Tooltip("The base offset of the NavMesh agent")]
    public float BaseOffset = 0;

    [Tooltip("The height of the NavMesh agent")]
    public float Height = 2f;
    [Tooltip("The obstacle avoidance type of the NavMesh agent")]
    public ObstacleAvoidanceType ObstacleAvoidanceType = ObstacleAvoidanceType.LowQualityObstacleAvoidance;
    [Tooltip("The radius of the NavMesh agent")]
    public float Radius = 0.5f;
    [Tooltip("The speed of the NavMesh agent")]
    public float Speed = 3f;
    [Tooltip("The stopping distance of the NavMesh agent")]
    public float StoppingDistance = 0.5f;

    [Header("Audio")]
    [Tooltip("The audio clip(s) played when the enemy groans.")]
    public AudioClip[] GroanAudioClips;
    [Tooltip("The audio clip(s) played when the enemy is walking.")]
    public AudioClip[] WalkAudioClips;
    [Tooltip("The audio clip(s) played when the enemy is running.")]
    public AudioClip[] RunAudioClips;
    [Tooltip("The audio clip(s) played when the enemy is jumping.")]
    public AudioClip[] JumpAudioClips;
    [Tooltip("The audio clip(s) played when the enemy attacks.")]
    public AudioClip[] AttackAudioClips;
    [Tooltip("The audio clip(s) played when the enemy is damaged.")]
    public AudioClip[] DamagedAudioClips;
    [Tooltip("The audio clip played when the enemy dies.")]
    public AudioClip DeathAudioClip;

    /// <summary>
    /// Scales up the enemy stats based on the provided scaling configuration and level.
    /// </summary>
    /// <param name="Scaling">Scaling configuration</param>
    /// <param name="Level">Level to scale up for</param>
    /// <returns>Scaled up enemy</returns>
    public EnemyScriptableObject ScaleUpForLevel(ScalingScriptableObject Scaling, int Level)
    {
        EnemyScriptableObject scaledUpEnemy = CreateInstance<EnemyScriptableObject>();

        scaledUpEnemy.name = name;
        scaledUpEnemy.Prefab = Prefab;

        scaledUpEnemy.AttackConfiguration = AttackConfiguration.ScaleUpForLevel(Scaling, Level);

        scaledUpEnemy.Health = Mathf.FloorToInt(Health * Scaling.HealthCurve.Evaluate(Level));

        scaledUpEnemy.DefaultState = DefaultState;
        scaledUpEnemy.IdleLocationRadius = IdleLocationRadius;
        scaledUpEnemy.IdleMovespeedMultiplier = IdleMovespeedMultiplier;
        scaledUpEnemy.Waypoints = Waypoints;
        scaledUpEnemy.LineOfSightRange = LineOfSightRange;
        scaledUpEnemy.FieldOfView = FieldOfView;

        scaledUpEnemy.AIUpdateInterval = AIUpdateInterval;
        scaledUpEnemy.Acceleration = Acceleration;
        scaledUpEnemy.AngularSpeed = AngularSpeed;

        scaledUpEnemy.AreaMask = AreaMask;
        scaledUpEnemy.AvoidancePriority = AvoidancePriority;

        scaledUpEnemy.BaseOffset = BaseOffset;
        scaledUpEnemy.Height = Height;
        scaledUpEnemy.ObstacleAvoidanceType = ObstacleAvoidanceType;
        scaledUpEnemy.Radius = Radius;
        scaledUpEnemy.Speed = Speed * Scaling.SpeedCurve.Evaluate(Level);
        scaledUpEnemy.StoppingDistance = StoppingDistance;

        // Copy audio clips
        scaledUpEnemy.GroanAudioClips = GroanAudioClips;
        scaledUpEnemy.WalkAudioClips = WalkAudioClips;
        scaledUpEnemy.RunAudioClips = RunAudioClips;
        scaledUpEnemy.JumpAudioClips = JumpAudioClips;
        scaledUpEnemy.AttackAudioClips = AttackAudioClips;
        scaledUpEnemy.DamagedAudioClips = DamagedAudioClips;
        scaledUpEnemy.DeathAudioClip = DeathAudioClip;

        return scaledUpEnemy;
    }

    public void SetUpEnemy(Enemy enemy)
    {
        // Set up NavMeshAgent properties
        enemy.Agent.acceleration = Acceleration;
        enemy.Agent.angularSpeed = AngularSpeed;
        enemy.Agent.areaMask = AreaMask;
        enemy.Agent.avoidancePriority = AvoidancePriority;
        enemy.Agent.baseOffset = BaseOffset;
        enemy.Agent.height = Height;
        enemy.Agent.obstacleAvoidanceType = ObstacleAvoidanceType;
        enemy.Agent.radius = Radius;
        enemy.Agent.speed = Speed;
        enemy.Agent.stoppingDistance = StoppingDistance;

        // Set up movement properties
        enemy.Movement.UpdateRate = AIUpdateInterval;
        enemy.Movement.DefaultState = DefaultState;
        enemy.Movement.IdleMovespeedMultiplier = IdleMovespeedMultiplier;
        enemy.Movement.IdleLocationRadius = IdleLocationRadius;
        enemy.Movement.Waypoints = new Vector3[Waypoints];
        enemy.Movement.LineOfSightChecker.FieldOfView = FieldOfView;
        enemy.Movement.LineOfSightChecker.Collider.radius = LineOfSightRange;
        enemy.Movement.LineOfSightChecker.LineOfSightLayers = AttackConfiguration.LineOfSightLayers;

        // Set up stats
        enemy.Health = Health;
        AttackConfiguration.SetupEnemy(enemy);

        // Set up audio clips
        enemy.DamagedAudioClips = DamagedAudioClips;
        enemy.Movement.GroanAudioClips = GroanAudioClips;
        enemy.MovementAudioHandler.WalkAudioClips = WalkAudioClips;
        enemy.MovementAudioHandler.RunAudioClips = RunAudioClips;
        enemy.MovementAudioHandler.JumpAudioClips = JumpAudioClips;
        enemy.MovementAudioHandler.AttackAudioClips = AttackAudioClips;
        enemy.MovementAudioHandler.DeathAudioClip = DeathAudioClip;
    }
}
