using UnityEngine;

public class Arrow : Projectile
{
    // Arrow uses gravity for arc trajectory
    // Make sure the prefab's Rigidbody has useGravity = true

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

        // Rotate arrow to face direction of travel
        if (_rb != null && _rb.linearVelocity.sqrMagnitude > 0.1f)
        {
            transform.rotation = Quaternion.LookRotation(_rb.linearVelocity);
        }
    }

    protected override void OnHit(Collider other)
    {
        Debug.Log($"Arrow hit: {other.gameObject.name}");
        base.OnHit(other);
    }
}
