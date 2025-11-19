// Adapted from: https://github.com/llamacademy/ai-series-part-4/blob/main/Assets/Scripts/Enemy/Enemy.cs

using UnityEngine;
using UnityEngine.AI;

public class Enemy : PoolableObject
{
    [Header("Components")]
    [Tooltip("The movement component of the enemy.")]
    public EnemyMovement Movement;
    [Tooltip("The NavMeshAgent component for the enemy.")]
    public NavMeshAgent Agent;
    [Tooltip("The scriptable object containing enemy configuration.")]
    public EnemyScriptableObject EnemyScriptableObject;

    [Header("Stats")]
    [Tooltip("The health of the enemy.")]
    public int Health = 100;

    public virtual void OnEnable()
    {
        SetupAgentFromConfiguration();
    }

    public override void OnDisable()
    {
        base.OnDisable();

        Agent.enabled = false;
    }

    public virtual void SetupAgentFromConfiguration()
    {
        Agent.acceleration = EnemyScriptableObject.Acceleration;
        Agent.angularSpeed = EnemyScriptableObject.AngularSpeed;
        Agent.areaMask = EnemyScriptableObject.AreaMask;
        Agent.avoidancePriority = EnemyScriptableObject.AvoidancePriority;
        Agent.baseOffset = EnemyScriptableObject.BaseOffset;
        Agent.height = EnemyScriptableObject.Height;
        Agent.obstacleAvoidanceType = EnemyScriptableObject.ObstacleAvoidanceType;
        Agent.radius = EnemyScriptableObject.Radius;
        Agent.speed = EnemyScriptableObject.Speed;
        Agent.stoppingDistance = EnemyScriptableObject.StoppingDistance;

        Movement.UpdateRate = EnemyScriptableObject.AIUpdateInterval;

        Health = EnemyScriptableObject.Health;
    }
}