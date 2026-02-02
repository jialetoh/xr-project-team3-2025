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

    // Camera Rig reference for local velocity calculation
    protected Transform _cameraRig;
    protected Vector3 _cameraRigLastPosition;

    // Startup grace period
    [Header("Swing Detection")]
    [Tooltip("Time after equipping before swing sounds can play")]
    public float swingStartupDelay = 0.15f;
    protected float _equipTime = float.MinValue;

    [Header("Debug Info")]
    [Tooltip("Show debug logs for swing detection")]
    public bool debugSwingDetection = false;

    protected virtual void Update()
    {
        // Don't calculate velocity until weapon has been equipped
        // _equipTime is initialized to float.MinValue, OnEquip() sets it to Time.time
        if (_equipTime == float.MinValue)
        {
            return;
        }

        // Calculate world velocity
        Vector3 worldVelocity = (transform.position - _lastPosition) / Time.deltaTime;
        _lastPosition = transform.position;

        // Calculate local velocity (subtract Camera Rig movement)
        if (_cameraRig != null)
        {
            Vector3 cameraRigVelocity = (_cameraRig.position - _cameraRigLastPosition) / Time.deltaTime;
            _cameraRigLastPosition = _cameraRig.position;

            // Local velocity = hand movement relative to body
            _velocity = worldVelocity - cameraRigVelocity;
        }
        else
        {
            // Fallback if Camera Rig not found
            _velocity = worldVelocity;
        }

        // Check and play swing sound
        CheckAndPlaySwingSound();
    }

    protected virtual void CheckAndPlaySwingSound()
    {
        // Don't play if no swing sounds assigned or audio source missing
        if (swingSounds == null || swingSounds.Length == 0 || swingAudioSource == null)
        {
            if (debugSwingDetection) Debug.Log("MeleeWeapon: No swing sounds or audio source");
            return;
        }

        // STARTUP GRACE PERIOD: Prevent false positives during initialization
        if (Time.time < _equipTime + swingStartupDelay)
        {
            if (debugSwingDetection) Debug.Log($"MeleeWeapon: In grace period. Time: {Time.time:F3}, Equip: {_equipTime:F3}, Delay: {swingStartupDelay:F3}");
            return;
        }

        float speed = _velocity.magnitude;

        if (debugSwingDetection && speed > 0.05f)
        {
            Debug.Log($"MeleeWeapon: Speed={speed:F3}, MinVelocity={swingMinVelocity:F3}, CooldownOK={Time.time >= _lastSwingTime + swingCooldown}, Velocity={_velocity}");
        }

        // Check if velocity exceeds minimum and cooldown has elapsed
        if (speed >= swingMinVelocity && Time.time >= _lastSwingTime + swingCooldown)
        {
            Vector3 currentDirection = _velocity.normalized;
            float dot = _lastSwingDirection == Vector3.zero ? -1f : Vector3.Dot(currentDirection, _lastSwingDirection);

            if (debugSwingDetection)
            {
                Debug.Log($"MeleeWeapon: Direction check - Dot={dot:F3}, Threshold={swingDirectionThreshold:F3}, LastDir={_lastSwingDirection}, CurDir={currentDirection}");
            }

            // Check if this is a new swing by comparing direction
            // A new swing means the direction has changed significantly
            if (_lastSwingDirection == Vector3.zero || dot < swingDirectionThreshold)
            {
                if (debugSwingDetection) Debug.Log($"MeleeWeapon: PLAYING SWING SOUND! Speed={speed:F3}");
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

    public override void OnEquip(bool playSound)
    {
        base.OnEquip(playSound);

        // Find and cache Camera Rig reference
        _cameraRig = FindCameraRig();

        // Initialize positions to prevent startup velocity spike
        _lastPosition = transform.position;
        _lastSwingDirection = Vector3.zero;

        // Initialize Camera Rig position tracking
        if (_cameraRig != null)
            _cameraRigLastPosition = _cameraRig.position;

        // Set equip time for grace period
        _equipTime = Time.time;
        _lastSwingTime = Time.time;
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
        IDamageable damageable = other.GetComponentInParent<IDamageable>();
        if (damageable != null) damageable.TakeDamage(damage);

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

    /// <summary>
    /// Finds the Camera Rig (player root) by traversing up the hierarchy.
    /// Weapon hierarchy: CameraRig -> TrackingSpace -> HandAnchor -> ControllerAnchor -> Interactor -> Weapon
    /// </summary>
    private Transform FindCameraRig()
    {
        Transform current = transform;
        int maxDepth = 10;
        int depth = 0;

        while (current.parent != null && depth < maxDepth)
        {
            current = current.parent;
            string name = current.name.ToLower();

            // Check for Camera Rig by name
            if (name.Contains("camerarig") || name.Contains("buildingblock") || name.Contains("playerrig"))
                return current;

            // Fallback: Check for Rigidbody at sufficient depth
            if (current.GetComponent<Rigidbody>() != null && depth >= 4)
                return current;

            depth++;
        }

        // Last resort: use root
        Transform root = transform.root;
        Debug.LogWarning($"MeleeWeapon: Could not find Camera Rig by name, using root: {root.name}");
        return root;
    }
}
