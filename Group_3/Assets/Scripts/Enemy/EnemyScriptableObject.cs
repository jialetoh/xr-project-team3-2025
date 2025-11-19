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
    [Header("Enemy Stats")]
    [Tooltip("The base health of the enemy")]
    public int Health = 100;
    [Tooltip("The time delay between enemy attacks")]
    public float AttackDelay = 1f;
    [Tooltip("The damage dealt by the enemy per attack")]
    public int Damage = 5;
    [Tooltip("The radius within which the enemy can attack")]
    public float AttackRadius = 1.5f;

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
}
