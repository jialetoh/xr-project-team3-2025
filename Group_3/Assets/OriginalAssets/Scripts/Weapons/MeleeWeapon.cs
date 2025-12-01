using UnityEngine;

public abstract class MeleeWeapon : Weapon
{
    [Header("Melee Parameters")]
    public int damage = 25;
    public float minHitVelocity = 1.0f;
    public LayerMask hittableLayers;

    [Header("Audio")]
    public AudioSource meleeAudioSource;
    public AudioClip hitSound;

    protected Vector3 _lastPosition;
    protected Vector3 _velocity;

    protected virtual void Update()
    {
        // Calculate velocity for swing detection
        _velocity = (transform.position - _lastPosition) / Time.deltaTime;
        _lastPosition = transform.position;
    }

    public override void OnEquip()
    {
        base.OnEquip();
        _lastPosition = transform.position;
    }

    protected virtual void OnTriggerEnter(Collider other)
    {
        // Only count hits if we're actually swinging
        if (_velocity.magnitude < minHitVelocity)
            return;

        // Layer filter
        if ((hittableLayers.value & (1 << other.gameObject.layer)) == 0)
            return;

        // Deal damage
        OnHit(other);
    }

    protected virtual void OnHit(Collider other)
    {
        // Override in subclasses for specific damage logic
        // IDamageable damageable = other.GetComponentInParent<IDamageable>();
        // if (damageable != null) damageable.TakeDamage(damage);

        // Play hit sound
        if (meleeAudioSource != null && hitSound != null)
            meleeAudioSource.PlayOneShot(hitSound);

        // Haptics
        HapticsManager.Instance?.PulseMeleeHitRight();
    }

    #region Input Handling

    public override void HandleLeftControllerInputs(LeftControllerRay ray)
    {
        MeleeInputHandler.HandleLeftControllerInputs(this, ray);
    }

    public override void HandleRightControllerInputs()
    {
        MeleeInputHandler.HandleRightControllerInputs(this);
    }

    #endregion
}
