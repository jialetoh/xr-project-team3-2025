// Adapted from: https://github.com/llamacademy/ai-series-part-4/blob/main/Assets/Scripts/Enemy/Enemy.cs

using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : PoolableObject, IDamageable
{
    [Header("Components")]
    [Tooltip("The attack radius component for the enemy.")]
    public AttackRadius AttackRadius;
    [Tooltip("The Animator component for the enemy.")]
    public Animator Animator;
    [Tooltip("The movement component of the enemy.")]
    public EnemyMovement Movement;
    [Tooltip("The NavMeshAgent component for the enemy.")]
    public NavMeshAgent Agent;
    [Tooltip("The scriptable object containing enemy configuration.")]
    public EnemyScriptableObject EnemyScriptableObject;

    [Header("Stats")]
    [Tooltip("The health of the enemy.")]
    public int Health = 100;
    [Tooltip("Coroutine handling the look process.")]
    private Coroutine LookCoroutine;
    [Tooltip("The attack trigger name in the Animator.")]
    private const string ATTACK_TRIGGER = "Attack";

    private void Awake()
    {
        AttackRadius.OnAttack += OnAttack;
    }

    private void OnAttack(IDamageable Target)
    {
        Animator.SetTrigger(ATTACK_TRIGGER);

        if (LookCoroutine != null)
        {
            StopCoroutine(LookCoroutine);
        }

        LookCoroutine = StartCoroutine(LookAt(Target.GetTransform()));
    }

    private IEnumerator LookAt(Transform Target)
    {
        Quaternion lookRotation = Quaternion.LookRotation(Target.position - transform.position);
        float time = 0;

        while (time < 1)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, time);

            time += Time.deltaTime * 2;
            yield return null;
        }

        transform.rotation = lookRotation;
    }

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
        (AttackRadius.Collider == null ? AttackRadius.GetComponent<SphereCollider>() : AttackRadius.Collider).radius = EnemyScriptableObject.AttackRadius;
        AttackRadius.AttackDelay = EnemyScriptableObject.AttackDelay;
        AttackRadius.Damage = EnemyScriptableObject.Damage;
    }

    public void TakeDamage(int Damage)
    {
        Health -= Damage;
        if (Health <= 0)
        {
            KillSelf();
        }
    }

    public Transform GetTransform()
    {
        return transform;
    }

    private void KillSelf()
    {
        // TODO: Play death animation
        gameObject.SetActive(false);
    }
}