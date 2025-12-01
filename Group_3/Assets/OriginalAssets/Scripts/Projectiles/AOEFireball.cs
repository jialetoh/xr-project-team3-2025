using UnityEngine;

public class AOEFireball : Projectile
{
    // AOE fireball uses gravity for arc trajectory
    // Make sure the prefab's Rigidbody has useGravity = true

    [Header("AOE Settings")]
    public float explosionRadius = 5f;
    public GameObject explosionEffectPrefab;

    private Rigidbody _rb;

    protected override void Start()
    {
        base.Start();
        _rb = GetComponent<Rigidbody>();

        // Ensure gravity is enabled for arc trajectory
        if (_rb != null)
        {
            _rb.useGravity = true;
        }
    }

    protected override void Update()
    {
        base.Update();

        // Rotate fireball to face direction of travel
        if (_rb != null && _rb.linearVelocity.sqrMagnitude > 0.1f)
        {
            transform.rotation = Quaternion.LookRotation(_rb.linearVelocity);
        }
    }

    protected override void OnHit(Collider other)
    {
        Debug.Log($"AOEFireball hit: {other.gameObject.name}");

        // Deal AOE damage
        Explode();

        // Spawn explosion effect
        if (explosionEffectPrefab != null)
        {
            Instantiate(explosionEffectPrefab, transform.position, Quaternion.identity);
        }

        // Play impact sound and destroy
        PlayImpactSound();
        Destroy(gameObject);
    }

    private void Explode()
    {
        // Find all colliders in explosion radius
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, explosionRadius, hitLayers);

        foreach (Collider hit in hitColliders)
        {
            // Deal damage to each target
            // IDamageable damageable = hit.GetComponentInParent<IDamageable>();
            // if (damageable != null) damageable.TakeDamage(damage);

            Debug.Log($"AOE damage to: {hit.gameObject.name}");
        }
    }

    // Visualize explosion radius in editor
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }

    #region Static Trajectory Helpers

    /// <summary>
    /// Calculate the predicted impact point for an AOE fireball trajectory
    /// </summary>
    public static Vector3 CalculateImpactPoint(Vector3 startPosition, Vector3 initialVelocity, LayerMask groundLayer, float maxTime = 5f)
    {
        return TrajectoryPreview.CalculateImpactPoint(startPosition, initialVelocity, groundLayer, maxTime);
    }

    /// <summary>
    /// Calculate trajectory arc points for visualization
    /// </summary>
    public static Vector3[] CalculateTrajectoryArc(Vector3 startPosition, Vector3 initialVelocity, int segments = 30, float timeStep = 0.1f)
    {
        return TrajectoryPreview.CalculateTrajectoryPoints(startPosition, initialVelocity, segments, timeStep);
    }

    #endregion
}
