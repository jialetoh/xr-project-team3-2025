// Adapted from: https://github.com/llamacademy/ai-series-part-4/blob/main/Assets/Scripts/Enemy/Enemy.cs

using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public enum EnemyTrigger
{
    Attack,
    Death,
};

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

    // Callback for when this enemy dies
    public delegate void DeathEvent(Enemy enemy);
    public DeathEvent OnDeath;
    private readonly static WaitForSeconds _waitForSeconds3 = new(3f);

    private void Awake()
    {
        AttackRadius.OnAttack += OnAttack;
    }

    private void OnAttack(IDamageable Target)
    {
        Animator.SetTrigger(EnemyTrigger.Attack.ToString());

        if (LookCoroutine != null)
        {
            StopCoroutine(LookCoroutine);
        }

        Vector3 targetPosition = Target.GetTransform().position;
        LookCoroutine = StartCoroutine(LookAt(transform, targetPosition, 0.2f));
    }

    private IEnumerator LookAt(Transform objectToMove, Vector3 worldPosition, float duration)
    {
        Quaternion currentRot = objectToMove.rotation;
        Quaternion newRot = Quaternion.LookRotation(worldPosition -
            objectToMove.position, objectToMove.TransformDirection(Vector3.up));

        float counter = 0;
        while (counter < duration)
        {
            counter += Time.deltaTime;
            objectToMove.rotation =
                Quaternion.Lerp(currentRot, newRot, counter / duration);
            yield return null;
        }
    }

    public override void OnDisable()
    {
        base.OnDisable();

        // Reset all states
        Agent.enabled = false; // Disable agent to re-enable on respawn
        OnDeath = null;
        AttackRadius.Reset();

        TryGetComponent(out Collider collider);
        if (collider != null && !collider.enabled)
            collider.enabled = true;
    }

    public void TakeDamage(int Damage)
    {
        Health -= Damage;
        if (Health <= 0)
        {
            StartCoroutine(KillSelf());
        }
    }

    public Transform GetTransform()
    {
        return transform;
    }

    private IEnumerator KillSelf()
    {
        // Disable movement and look behavior
        if (LookCoroutine != null)
        {
            StopCoroutine(LookCoroutine);
        }

        TryGetComponent(out Collider collider);
        if (collider != null && collider.isTrigger)
            collider.enabled = false;

        // Trigger the death animation
        Animator.SetTrigger(EnemyTrigger.Death.ToString());
        // Notify spawner via callback
        OnDeath?.Invoke(this);

        // Wait for death animation to play (3 seconds)
        yield return _waitForSeconds3;

        // Despawn the enemy
        gameObject.SetActive(false);
    }
}