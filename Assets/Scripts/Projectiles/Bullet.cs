using UnityEngine;

public class Bullet : Projectile
{
    // Bullet travels in a straight line (no gravity by default)
    // Velocity is set by the weapon when instantiated

    protected override void OnHit(Collider other)
    {
        Debug.Log($"Bullet hit: {other.gameObject.name}");
        base.OnHit(other);
    }
}
