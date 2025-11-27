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
    public GameObject ammoPrefab;        // Prefab for AmmoInteractable (magazine, shell, etc)
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
    public AudioClip rackSlideClip;
    public AudioClip roundEjectClip;

    [Header("Interactables")]
    public PullableInteractable pullableInteractable;

    [Header("Audio")]
    public AudioSource gunAudioSource;
    public AudioClip shootClip;
    public AudioClip dryFireClip;
    public AudioClip reloadClip;
    public AudioClip magEjectClip;

    protected float _nextFireTime;

    public override void OnEquip()
    {
        base.OnEquip();
        _nextFireTime = 0f;
        ammoInMagazine = Mathf.Clamp(ammoInMagazine, 0, magazineSize);
        ammoReserve = Mathf.Max(ammoReserve, 0);

        // Initialize pullable interactable reference if not set
        if (pullableInteractable == null)
            pullableInteractable = GetComponentInChildren<PullableInteractable>();

        // Update pullable visual state
        UpdatePullableVisualState();
    }

    protected virtual void UpdatePullableVisualState()
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
            Debug.Log($"Rigidbody found, setting velocity to {bulletSpeed}");
            rb.linearVelocity = shotRotation * Vector3.forward * bulletSpeed;
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
        if (gunAudioSource != null && dryFireClip != null)
            gunAudioSource.PlayOneShot(dryFireClip);

        HapticsManager.Instance?.PulseDryFireRight();
    }

    protected virtual void PlayShootEffects()
    {
        if (gunAudioSource != null && shootClip != null)
            gunAudioSource.PlayOneShot(shootClip);

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

        if (gunAudioSource != null && reloadClip != null)
            gunAudioSource.PlayOneShot(reloadClip);
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

        if (gunAudioSource != null && magEjectClip != null)
            gunAudioSource.PlayOneShot(magEjectClip);
    }

    protected virtual void DropMagazine()
    {
        if (magazineVisual == null) return;

        // Store current world position/rotation before unparenting
        Vector3 worldPos = magazineVisual.transform.position;
        Quaternion worldRot = magazineVisual.transform.rotation;

        // Detach from gun
        magazineVisual.transform.SetParent(null);
        magazineVisual.transform.SetPositionAndRotation(worldPos, worldRot);

        // Add Rigidbody if not present
        Rigidbody rb = magazineVisual.GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = magazineVisual.AddComponent<Rigidbody>();
        }
        rb.isKinematic = false;

        // Add Collider if not present
        if (magazineVisual.GetComponent<Collider>() == null)
        {
            magazineVisual.AddComponent<BoxCollider>();
        }

        // Apply ejection force (downward)
        rb.linearVelocity = -transform.up * magazineEjectForce;

        // Destroy after a delay to clean up
        Destroy(magazineVisual, 5f);

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

        if (gunAudioSource != null && reloadClip != null)
            gunAudioSource.PlayOneShot(reloadClip);

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
        // Remove any physics components
        Rigidbody rb = ammoObject.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
            Destroy(rb);
        }

        // Parent to gun at the insert point
        Transform attachPoint = ammoInsertPoint != null ? ammoInsertPoint : transform;
        ammoObject.transform.SetParent(attachPoint);
        ammoObject.transform.localPosition = Vector3.zero;
        ammoObject.transform.localRotation = Quaternion.identity;

        // Update reference
        magazineVisual = ammoObject;

        Debug.Log($"{GetType().Name}: Magazine visual attached");
    }

    #endregion

    #region Chamber / Slide Mechanics

    /// <summary>
    /// Called when the slide/charging handle is pulled and released.
    /// Always interactable. Ejects chambered round if present, then chambers from magazine.
    /// </summary>
    public virtual void RackSlide()
    {
        // If there's a chambered round, eject it (wasteful but realistic)
        if (hasRoundChambered)
        {
            hasRoundChambered = false;
            PlayRoundEjectSound();
            Debug.Log($"{GetType().Name}: Ejected live round from chamber");
        }

        // Try to chamber a round from the magazine
        ChamberRoundFromMagazine();

        PlayRackSlideEffects();
        UpdatePullableVisualState();
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

    protected virtual void PlayRackSlideEffects()
    {
        if (gunAudioSource != null && rackSlideClip != null)
            gunAudioSource.PlayOneShot(rackSlideClip);

        HapticsManager.Instance?.PulseSlideRackRight();
    }

    protected virtual void PlayRoundEjectSound()
    {
        if (gunAudioSource != null && roundEjectClip != null)
            gunAudioSource.PlayOneShot(roundEjectClip);
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
