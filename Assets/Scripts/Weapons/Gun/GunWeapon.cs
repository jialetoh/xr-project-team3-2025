using UnityEngine;

public abstract class GunWeapon : Weapon
{
    [Header("References")]
    public Transform muzzleTransform;
    public GameObject projectilePrefab;

    [Header("Ballistics")]
    public float bulletSpeed = 50f;
    public float fireRate = 2f;          // shots per second
    public float spreadAngle = 1f;       // random spread in degrees

    [Header("Ammo")]
    public int magazineSize = 12;
    public int ammoInMagazine = 12;
    public int ammoReserve = 36;         // total spare rounds
    public GameObject grabbableAmmoPrefab;  // Prefab for AmmoInteractable (magazine, shell, etc)
    public Transform ammoInsertPoint;    // Where ammo snaps into gun

    [Header("Magazine Visual")]
    [Tooltip("The actual magazine GameObject that is part of this gun prefab. Will be detached and dropped on eject.")]
    public GameObject magazineVisual;         // The magazine mesh/object in the gun prefab
    public float magazineEjectForce = 0.5f;   // Downward force when ejecting

    [Header("Chamber")]
    public bool hasRoundChambered = true;
    public bool isBoltLocked = false;
    [Tooltip("If true, the pullable (slide/handle) visually locks back when empty. True for pistols, false for rifles with separate charging handles.")]
    public bool pullableLocksBack = true;

    [Header("Ejection Port")]
    [Tooltip("Transform where ejected rounds spawn. Should be positioned at ejection port on gun model. Configure per-gun in Inspector.")]
    public Transform ejectionPort;
    public float ejectionForce = 2f;       // Lateral ejection velocity
    public float ejectionUpwardForce = 1f; // Upward component

    [Header("Interactables")]
    public PullableInteractable pullableInteractable;

    [Header("Audio")]
    public AudioClip shootClip;
    public AudioClip dryFireClip;
    public AudioClip magEjectClip;
    public AudioClip roundEjectClip;

    protected float _nextFireTime;

    public override void OnEquip(bool playSound)
    {
        base.OnEquip(playSound);
        _nextFireTime = 0f;

        // FIX: Guns always start with empty chamber - player must cock the gun first
        hasRoundChambered = false;

        ammoInMagazine = Mathf.Clamp(ammoInMagazine, 0, magazineSize);
        ammoReserve = Mathf.Max(ammoReserve, 0);

        // Initialize pullable interactable reference if not set
        if (pullableInteractable == null)
            pullableInteractable = GetComponentInChildren<PullableInteractable>();

        // Update pullable visual state
        UpdatePullableVisualState();
    }

    public virtual void UpdatePullableVisualState()
    {
        if (pullableInteractable == null) return;

        // Only lock back visually if pullableLocksBack is true (pistols)
        if (isBoltLocked && pullableLocksBack)
            pullableInteractable.LockBack();
        else
            pullableInteractable.ReturnToRest();
    }

    // Fire input methods - to be implemented by subclasses
    public abstract void OnPrimaryFireDown();
    public abstract void OnPrimaryFireUp();

    public virtual void OnReload()
    {
        InsertMagazine();
    }

    public virtual void OnAltAction()
    {
        EjectMagazine();
    }

    #region Firing

    protected virtual bool CanFire()
    {
        return Time.time >= _nextFireTime && ammoInMagazine > 0;
    }

    protected virtual void Fire()
    {
        if (muzzleTransform == null)
        {
            Debug.LogWarning($"{GetType().Name}: Missing muzzleTransform.");
            return;
        }
        if (projectilePrefab == null)
        {
            Debug.LogWarning($"{GetType().Name}: Missing projectilePrefab.");
            return;
        }

        _nextFireTime = Time.time + (1f / fireRate);

        // Spawn projectile with spread
        Quaternion shotRotation = muzzleTransform.rotation;
        if (spreadAngle > 0f)
        {
            shotRotation *= Quaternion.Euler(
                Random.Range(-spreadAngle, spreadAngle),
                Random.Range(-spreadAngle, spreadAngle),
                0f
            );
        }

        GameObject projObj = Instantiate(projectilePrefab, muzzleTransform.position, shotRotation);
        Debug.Log($"Projectile spawned: {projObj.name} at {muzzleTransform.position}");
        if (projObj.TryGetComponent<Rigidbody>(out var rb))
        {
            Debug.Log($"Rigidbody found, applying force {bulletSpeed}");
            rb.AddForce(shotRotation * Vector3.forward * bulletSpeed, ForceMode.VelocityChange);
        }
        else
        {
            Debug.LogWarning($"No Rigidbody found on projectile {projObj.name}!");
        }

        // Consume ammo
        ammoInMagazine--;

        // Effects
        PlayShootEffects();
    }

    protected virtual void PlayDryFire()
    {
        if (weaponAudioSource != null && dryFireClip != null)
            weaponAudioSource.PlayOneShot(dryFireClip);

        HapticsManager.Instance?.PulseDryFireRight();
    }

    protected virtual void PlayShootEffects()
    {
        if (weaponAudioSource != null && shootClip != null)
            weaponAudioSource.PlayOneShot(shootClip);

        HapticsManager.Instance?.PulseShootRight();
    }

    #endregion

    #region Ammo Management

    public virtual void InsertMagazine()
    {
        if (ammoInMagazine > 0)
            return;

        if (ammoReserve <= 0)
            return;

        int toLoad = Mathf.Min(magazineSize, ammoReserve);
        ammoInMagazine = toLoad;
        ammoReserve -= toLoad;

        // Audio is handled by AmmoInteractable
    }

    public virtual void EjectMagazine()
    {
        if (magazineVisual == null || !magazineVisual.activeInHierarchy)
        {
            Debug.Log($"{GetType().Name}: No magazine to eject");
            return;
        }

        // Return leftover rounds to reserve
        ammoReserve += ammoInMagazine;
        ammoInMagazine = 0;

        // Detach and drop the actual magazine
        DropMagazine();

        if (weaponAudioSource != null && magEjectClip != null)
            weaponAudioSource.PlayOneShot(magEjectClip);
    }

    protected virtual void DropMagazine()
    {
        if (magazineVisual == null) return;

        // Store current world position/rotation
        Vector3 worldPos = magazineVisual.transform.position;
        Quaternion worldRot = magazineVisual.transform.rotation;

        // HIDE original magazine visual FIRST (before spawning duplicate)
        magazineVisual.SetActive(false);

        // Create duplicate magazine to drop
        GameObject droppedMag = Instantiate(magazineVisual, worldPos, worldRot);

        // Re-enable the dropped magazine (it was inactive when instantiated)
        droppedMag.SetActive(true);

        // Unparent from gun immediately
        droppedMag.transform.SetParent(null);

        // Add physics to dropped magazine
        Rigidbody rb = droppedMag.GetComponent<Rigidbody>();
        if (rb == null)
            rb = droppedMag.AddComponent<Rigidbody>();

        rb.isKinematic = false;
        rb.useGravity = true;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous; // Prevent clipping

        // Add collider if missing
        if (droppedMag.GetComponent<Collider>() == null)
        {
            BoxCollider col = droppedMag.AddComponent<BoxCollider>();
            // Optionally adjust collider size if needed
        }

        // Apply ejection force (downward and slightly away from gun)
        Vector3 ejectionVelocity = -transform.up * magazineEjectForce + transform.forward * 0.1f;
        rb.linearVelocity = ejectionVelocity;

        // Add slight rotation for realism
        rb.angularVelocity = Random.insideUnitSphere * 2f;

        // Destroy dropped magazine after delay
        Destroy(droppedMag, 5f);

        Debug.Log($"{GetType().Name}: Magazine ejected");
    }

    /// <summary>
    /// Check if ammo can be inserted (called by AmmoInteractable before inserting).
    /// Returns true if magazine has been ejected.
    /// </summary>
    public virtual bool CanInsertAmmo()
    {
        // Can insert if magazine is missing (was ejected)
        return magazineVisual == null || !magazineVisual.activeInHierarchy;
    }

    /// <summary>
    /// Insert ammo into the gun. Called when AmmoInteractable is inserted.
    /// The AmmoInteractable becomes the new magazine visual.
    /// Handles game logic only - audio and haptics are handled by AmmoInteractable.
    /// </summary>
    public virtual void InsertAmmo(int amount, GameObject ammoObject = null)
    {
        if (!CanInsertAmmo())
        {
            Debug.Log($"{GetType().Name}: Magazine already inserted");
            return;
        }

        ammoInMagazine = Mathf.Min(amount, magazineSize);

        // If an ammo object was provided, attach it as the new magazine visual
        if (ammoObject != null)
        {
            AttachMagazineVisual(ammoObject);
        }

        Debug.Log($"{GetType().Name}: Magazine inserted with {ammoInMagazine} rounds");
    }

    /// <summary>
    /// Legacy overload for backward compatibility.
    /// </summary>
    public virtual void InsertAmmo(int amount)
    {
        InsertAmmo(amount, null);
    }

    /// <summary>
    /// Attach an ammo object as the new magazine visual.
    /// </summary>
    protected virtual void AttachMagazineVisual(GameObject ammoObject)
    {
        // Show the original magazine visual again
        if (magazineVisual != null)
        {
            magazineVisual.SetActive(true);
        }

        // Destroy the AmmoInteractable that was inserted (it's now "consumed")
        Destroy(ammoObject);

        Debug.Log($"{GetType().Name}: Magazine reattached");
    }

    #endregion

    #region Chamber / Pullable Mechanics

    /// <summary>
    /// Called when the pullable (slide/charging handle) is pulled back past threshold.
    /// Handles game logic only - audio is handled by PullableInteractable.
    /// </summary>
    public virtual void OnPullablePulled()
    {
        // If there's a chambered round, eject it (wasteful but realistic)
        if (hasRoundChambered)
        {
            hasRoundChambered = false;
            PlayRoundEjectSound();
            Debug.Log($"{GetType().Name}: Ejected live round from chamber");
        }

        HapticsManager.Instance?.PulseSlideRackRight();
    }

    /// <summary>
    /// Called when the pullable (slide/charging handle) is released.
    /// Chambers a round from the magazine.
    /// Handles game logic only - audio is handled by PullableInteractable.
    /// </summary>
    public virtual void OnPullableReleased()
    {
        // Try to chamber a round from the magazine
        ChamberRoundFromMagazine();
        UpdatePullableVisualState();
    }

    /// <summary>
    /// Legacy method for backward compatibility. Calls both pull and release.
    /// </summary>
    public virtual void RackSlide()
    {
        OnPullablePulled();
        OnPullableReleased();
    }

    /// <summary>
    /// Attempts to chamber a round from the magazine.
    /// </summary>
    protected virtual void ChamberRoundFromMagazine()
    {
        if (ammoInMagazine > 0)
        {
            ammoInMagazine--;
            hasRoundChambered = true;
            isBoltLocked = false;
            Debug.Log($"{GetType().Name}: Chambered round. Ammo remaining: {ammoInMagazine}");
        }
        else
        {
            hasRoundChambered = false;
            Debug.Log($"{GetType().Name}: No ammo to chamber");
        }
    }

    /// <summary>
    /// Called after firing when magazine is empty. Locks the bolt back.
    /// </summary>
    protected virtual void LockBoltBack()
    {
        isBoltLocked = true;
        hasRoundChambered = false;

        // Only visually lock back if pullableLocksBack is true (pistols)
        if (pullableLocksBack && pullableInteractable != null)
            pullableInteractable.LockBack();

        Debug.Log($"{GetType().Name}: Bolt locked back - empty magazine");
    }

    /// <summary>
    /// Called after firing to consume the chambered round and feed the next.
    /// Override in subclasses for different behavior (semi-auto vs full-auto cycling).
    /// </summary>
    protected virtual void ConsumeChamberedRound()
    {
        if (ammoInMagazine > 0)
        {
            ammoInMagazine--;
            hasRoundChambered = true;
            isBoltLocked = false;
        }
        else
        {
            hasRoundChambered = false;
            LockBoltBack();
        }
    }

    protected virtual void PlayRoundEjectSound()
    {
        if (weaponAudioSource != null && roundEjectClip != null)
            weaponAudioSource.PlayOneShot(roundEjectClip);

        // Spawn visual ejected round
        SpawnEjectedRound();
    }

    protected virtual void SpawnEjectedRound()
    {
        if (projectilePrefab == null || ejectionPort == null)
            return;

        // Spawn projectile prefab as ejected round
        GameObject ejectedRound = Instantiate(
            projectilePrefab,
            ejectionPort.position,
            ejectionPort.rotation
        );

        // Remove any Projectile script components (so it doesn't deal damage)
        Projectile projScript = ejectedRound.GetComponent<Projectile>();
        if (projScript != null)
            Destroy(projScript);

        // Apply physics
        Rigidbody rb = ejectedRound.GetComponent<Rigidbody>();
        if (rb == null)
            rb = ejectedRound.AddComponent<Rigidbody>();

        rb.useGravity = true; // Follow gravity

        // Eject to the right and slightly upward
        Vector3 ejectionDirection = transform.right + transform.up * 0.5f;
        rb.linearVelocity = ejectionDirection.normalized * ejectionForce + Vector3.up * ejectionUpwardForce;

        // Add random spin
        rb.angularVelocity = Random.insideUnitSphere * 10f;

        // Auto-cleanup
        Destroy(ejectedRound, 10f);
    }

    #endregion

    #region Input Handling

    public override void HandleLeftControllerInputs(LeftControllerRay ray)
    {
        GunInputHandler.HandleLeftControllerInputs(this, ray);
    }

    public override void HandleRightControllerInputs()
    {
        GunInputHandler.HandleRightControllerInputs(this);
    }

    public override void ResetInputState()
    {
        GunInputHandler.ResetState();
    }

    #endregion
}
