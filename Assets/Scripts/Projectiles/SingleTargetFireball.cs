using UnityEngine;

public class SingleTargetFireball : Projectile
{
    // Single-target fireball travels in a straight line (no gravity)
    // Velocity is set by the weapon when instantiated

    [Header("Fireball Effects")]
    public GameObject impactEffectPrefab;

    protected override void OnHit(Collider other)
    {
        Debug.Log($"SingleTargetFireball hit: {other.gameObject.name}");

        // Spawn impact effect
        if (impactEffectPrefab != null)
        {
            Instantiate(impactEffectPrefab, transform.position, Quaternion.identity);
        }

        base.OnHit(other);
    }
}
