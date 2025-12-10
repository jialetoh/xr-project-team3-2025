// Adapted from: https://github.com/llamacademy/ai-series-part-7/blob/main/Assets/Scripts/RangedAttackRadius.cs

using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class RangedAttackRadius : AttackRadius
{
    [Header("Components")]
    [Tooltip("The NavMeshAgent component used for movement.")]
    public NavMeshAgent Agent;
    [Tooltip("The Fireball prefab to be used for ranged attacks.")]
    public Fireball FireballPrefab;
    [Tooltip("The height offset for spawning the fireball.")]
    public float SpawnHeightOffset = 1f;
    [Tooltip("The layer mask used for line of sight checks.")]
    public LayerMask Mask;

    [Header("Predictive Targeting")]
    [Tooltip("Chance (0-1) that the fireball will use predictive targeting instead of direct targeting.")]
    [Range(0f, 1f)]
    public float PredictiveChance = 0.5f;
    [Tooltip("How far ahead to predict the target's movement (in seconds).")]
    public float PredictionTime = 1.0f;
    [Tooltip("Assumed movement speed when target direction is used for prediction (fallback when no velocity detected).")]
    public float AssumedTargetSpeed = 5.0f;
    [Tooltip("Height offset to aim at target's center/torso instead of feet.")]
    public float TargetHeightOffset = 1.0f;

    private ObjectPool FireballPool;
    [SerializeField]
    private float SphereCastRadius = 0.45f; // Based on the fireball radius
    private RaycastHit Hit;
    private Fireball fireball;

    // Dictionary to store previous positions for velocity calculation
    private System.Collections.Generic.Dictionary<Transform, Vector3> previousPositions = new();
    private System.Collections.Generic.Dictionary<Transform, float> lastUpdateTimes = new();

    public void CreateFireballPool()
    {
        if (FireballPool == null)
            FireballPool = ObjectPool.CreateInstance(
                FireballPrefab,
                Mathf.CeilToInt(FireballPrefab.AutoDestroyTime / AttackDelay)
            );
    }

    protected override IEnumerator Attack()
    {
        WaitForSeconds Wait = new(AttackDelay);

        while (Damageables.Count > 0)
        {
            IDamageable targetDamageable = null;

            // Find the first damageable with line of sight
            for (int i = 0; i < Damageables.Count; i++)
            {
                if (HasLineOfSightTo(Damageables[i].GetTransform()))
                {
                    targetDamageable = Damageables[i];
                    break;
                }
            }

            Agent.enabled = targetDamageable == null;
            if (targetDamageable != null)
            {
                OnAttack?.Invoke(targetDamageable);
                PlayAttackSound();
                LaunchFireballToward(targetDamageable.GetTransform());
            }

            yield return Wait;

            if (targetDamageable == null || !HasLineOfSightTo(targetDamageable.GetTransform()))
            {
                Agent.enabled = true;
            }

            Damageables.RemoveAll(DisabledDamageables);
        }

        Agent.enabled = true;
        AttackCoroutine = null;
    }

    private Vector3 FindSpawnOffset()
    {
        // Calculate spawn offset based on enemy's forward direction in the x and z direction
        // This ensures the fireball spawns in front of the enemy while maintaining the y offset
        Vector3 forward = transform.forward;
        Vector3 horizontalForward = new Vector3(forward.x, 0, forward.z / 2).normalized;
        return horizontalForward + new Vector3(0, SpawnHeightOffset, 0);
    }

    private bool HasLineOfSightTo(Transform Target)
    {
        Vector3 origin = transform.position + FindSpawnOffset();
        Vector3 targetPos = Target.position + FindSpawnOffset();
        Vector3 dir = (targetPos - origin).normalized;
        float distance = Vector3.Distance(origin, targetPos);

        // If target is essentially at the origin, consider it visible
        if (distance <= Mathf.Epsilon)
        {
            return true;
        }

        // SphereCast out to the target distance to detect obstructions
        bool hitSomething = Physics.SphereCast(origin, SphereCastRadius, dir, out Hit, distance, Mask);

        if (hitSomething)
        {
            // Resolve IDamageable on the hit collider or its parents (targets may have the component on parent)
            IDamageable damageable = Hit.collider.GetComponentInParent<IDamageable>();
            if (damageable != null)
            {
                return damageable.GetTransform() == Target;
            }

            // If the hit collider's transform is exactly the target, accept it
            return Hit.collider.transform == Target;
        }

        // Nothing hit between origin and target -> clear line of sight
        return true;
    }

    protected override void OnTriggerExit(Collider other)
    {
        base.OnTriggerExit(other);
        if (AttackCoroutine == null)
        {
            Agent.enabled = true;
        }
    }

    private void LaunchFireballToward(Transform target)
    {
        PoolableObject poolableObject = FireballPool.GetObject();
        if (poolableObject != null)
        {
            fireball = poolableObject.GetComponent<Fireball>();

            // Calculate spawn position
            Vector3 spawnPosition = transform.position + FindSpawnOffset();

            // Determine target position (direct or predictive)
            Vector3 targetPosition = CalculateTargetPosition(target);

            // Calculate direction toward target (direct or predicted)
            Vector3 targetDirection = (targetPosition - spawnPosition).normalized;
            Quaternion targetRotation = Quaternion.LookRotation(targetDirection);

            // Setup fireball properties
            fireball.Damage = Damage;
            fireball.transform.SetPositionAndRotation(spawnPosition, targetRotation);

            // Launch fireball toward target
            fireball.Rigidbody.AddForce(
                targetDirection * FireballPrefab.MoveSpeed,
                ForceMode.VelocityChange
            );
        }
    }

    private Vector3 CalculateTargetPosition(Transform target)
    {
        // Random chance to use predictive targeting
        bool usePredictive = Random.Range(0f, 1f) < PredictiveChance;

        Vector3 baseTargetPosition = target.position + Vector3.up * TargetHeightOffset;

        if (!usePredictive)
        {
            // Direct targeting - aim at current position with height offset
            return baseTargetPosition;
        }

        // Predictive targeting - calculate target velocity and predict future position
        Vector3 targetVelocity = CalculateTargetVelocity(target);

        // If we can't determine velocity (first frame, no movement), fall back to direct targeting
        if (targetVelocity == Vector3.zero)
        {
            return baseTargetPosition;
        }

        // Calculate predicted position with height offset
        Vector3 predictedPosition = baseTargetPosition + (targetVelocity * PredictionTime);

        return predictedPosition;
    }

    private Vector3 CalculateTargetVelocity(Transform target)
    {
        // Try to get velocity from various components on the target
        Vector3 velocity = Vector3.zero;

        // Method 1: Check for NavMeshAgent (for AI enemies or NPCs)
        NavMeshAgent targetAgent = target.GetComponent<NavMeshAgent>();
        if (targetAgent != null && targetAgent.enabled)
        {
            velocity = targetAgent.velocity;
        }
        // Method 2: Check for Rigidbody (for physics-based movement)
        else
        {
            Rigidbody targetRb = target.GetComponent<Rigidbody>();
            if (targetRb != null)
            {
                velocity = targetRb.linearVelocity;
            }
        }

        // Method 3: If no component-based velocity, calculate from position changes
        if (velocity.magnitude < 0.1f) // Very small velocity, try position-based calculation
        {
            velocity = CalculateVelocityFromPositionChange(target);
        }

        // Method 4: If still no good velocity data, use target's forward direction as movement prediction
        if (velocity.magnitude < 0.1f)
        {
            // Assume target is moving in the direction they're facing
            velocity = target.forward * AssumedTargetSpeed;
        }

        return velocity;
    }

    private Vector3 CalculateVelocityFromPositionChange(Transform target)
    {
        float currentTime = Time.time;

        // Check if we have previous data for this target
        if (previousPositions.ContainsKey(target) && lastUpdateTimes.ContainsKey(target))
        {
            Vector3 previousPosition = previousPositions[target];
            float previousTime = lastUpdateTimes[target];
            float deltaTime = currentTime - previousTime;

            if (deltaTime > 0)
            {
                Vector3 velocity = (target.position - previousPosition) / deltaTime;

                // Update stored values
                previousPositions[target] = target.position;
                lastUpdateTimes[target] = currentTime;

                return velocity;
            }
        }

        // Store current position and time for next calculation
        previousPositions[target] = target.position;
        lastUpdateTimes[target] = currentTime;

        return Vector3.zero; // No velocity data available yet
    }
}