using UnityEngine;

public abstract class MeleeWeapon : Weapon
{
    [Header("Melee Parameters")]
    public int damage = 25;
    public float minHitVelocity = 0.2f;
    public LayerMask hittableLayers;

    [Header("Audio Sources")]
    public AudioSource swingAudioSource;
    public AudioSource impactAudioSource;
    public AudioSource dragAudioSource;

    [Header("Swing Audio")]
    public AudioClip[] swingSounds;
    public float swingMinVelocity = 0.2f;
    public float swingMaxVelocity = 6.0f;
    public float swingCooldown = 0.2f;
    public float swingPitchVariation = 0.15f;
    public float swingDirectionThreshold = 0.5f; // Minimum direction change to count as new swing
    private int _swingIndex = 0;
    private float _lastSwingTime;
    private Vector3 _lastSwingDirection;

    [Header("Impact Audio")]
    public AudioClip[] impactSounds;
    public float impactPitchVariation = 0.15f;
    public float impactVolumeMin = 0.8f;
    public float impactVolumeMax = 1.0f;

    [Header("Drag Audio")]
    public AudioClip dragSound;
    protected bool _isDragging = false;

    protected Vector3 _lastPosition;
    protected Vector3 _velocity;

    protected virtual void Update()
    {
        // Calculate velocity for swing detection
        _velocity = (transform.position - _lastPosition) / Time.deltaTime;
        _lastPosition = transform.position;

        // Check and play swing sound
        CheckAndPlaySwingSound();
    }

    protected virtual void CheckAndPlaySwingSound()
    {
        // Don't play if no swing sounds assigned or audio source missing
        if (swingSounds == null || swingSounds.Length == 0 || swingAudioSource == null)
            return;

        float speed = _velocity.magnitude;

        // Check if velocity exceeds minimum and cooldown has elapsed
        if (speed >= swingMinVelocity && Time.time >= _lastSwingTime + swingCooldown)
        {
            Vector3 currentDirection = _velocity.normalized;

            // Check if this is a new swing by comparing direction
            // A new swing means the direction has changed significantly
            if (_lastSwingDirection == Vector3.zero || Vector3.Dot(currentDirection, _lastSwingDirection) < swingDirectionThreshold)
            {
                PlaySwingSound(speed);
                _lastSwingTime = Time.time;
                _lastSwingDirection = currentDirection;
            }
        }
    }

    protected virtual void PlaySwingSound(float velocity)
    {
        // Get clip using strict alternation
        AudioClip clip = swingSounds[_swingIndex];
        _swingIndex = (_swingIndex + 1) % swingSounds.Length;

        // Calculate volume based on velocity
        float normalizedVelocity = Mathf.Clamp01((velocity - swingMinVelocity) / (swingMaxVelocity - swingMinVelocity));
        float volume = Mathf.Lerp(0.7f, 1.0f, normalizedVelocity);

        // Apply pitch variation
        float pitch = 1.0f + Random.Range(-swingPitchVariation, swingPitchVariation);

        swingAudioSource.pitch = pitch;
        swingAudioSource.PlayOneShot(clip, volume);
    }

    public override void OnEquip()
    {
        base.OnEquip();
        _lastPosition = transform.position;
        _lastSwingDirection = Vector3.zero; // Reset swing direction on equip
        _lastSwingTime = Time.time; // Prevent immediate swing sound on equip
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

    protected virtual void OnTriggerStay(Collider other)
    {
        // Base implementation: no drag sound
        // Override in Sword and Lightsaber to enable drag sounds
    }

    protected virtual void OnTriggerExit(Collider other)
    {
        // Stop drag sound if playing
        if (_isDragging && dragAudioSource != null && dragAudioSource.isPlaying)
        {
            dragAudioSource.Stop();
            _isDragging = false;
        }
    }

    protected virtual void OnHit(Collider other)
    {
        // Override in subclasses for specific damage logic
        // IDamageable damageable = other.GetComponentInParent<IDamageable>();
        // if (damageable != null) damageable.TakeDamage(damage);

        // Play impact sound with velocity-based variation
        PlayImpactSound(_velocity.magnitude);

        // Haptics
        HapticsManager.Instance?.PulseMeleeHitRight();
    }

    protected virtual void PlayImpactSound(float velocity)
    {
        if (impactSounds == null || impactSounds.Length == 0 || impactAudioSource == null)
            return;

        // Randomly select impact clip
        AudioClip clip = impactSounds[Random.Range(0, impactSounds.Length)];

        // Calculate volume based on impact velocity
        float normalizedVelocity = Mathf.Clamp01((velocity - minHitVelocity) / (swingMaxVelocity - minHitVelocity));
        float volume = Mathf.Lerp(impactVolumeMin, impactVolumeMax, normalizedVelocity);

        // Apply pitch variation
        float pitch = 1.0f + Random.Range(-impactPitchVariation, impactPitchVariation);

        impactAudioSource.pitch = pitch;
        impactAudioSource.PlayOneShot(clip, volume);
    }

    public override void HandleLeftControllerInputs(LeftControllerRay ray)
    {
        MeleeInputHandler.HandleLeftControllerInputs(this, ray);
    }

    public override void HandleRightControllerInputs()
    {
        MeleeInputHandler.HandleRightControllerInputs(this);
    }
}
