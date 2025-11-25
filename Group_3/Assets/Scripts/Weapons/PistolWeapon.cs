using UnityEngine;

public class PistolWeapon : Weapon
{
    [Header("References")]
    public Transform muzzleTransform;
    public GameObject projectilePrefab;

    [Header("Ballistics")]
    public float bulletSpeed = 50f;
    public float fireRate = 2f;          // shots per second
    public float spreadAngle = 1f;       // small random spread

    [Header("Ammo")]
    public int magazineSize = 12;
    public int ammoInMagazine = 12;
    public int ammoReserve = 36;         // total spare rounds
    public bool hasRoundChambered = true;

    [Header("Audio")]
    public AudioSource gunAudioSource;
    public AudioClip shootClip;
    public AudioClip dryFireClip;
    public AudioClip reloadClip;
    public AudioClip rackSlideClip;
    public AudioClip magEjectClip;

    private RightControllerInteractor _rightInteractor;
    private float _nextFireTime;


    private void Awake()
    {
        _rightInteractor = GetComponentInParent<RightControllerInteractor>();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {

    }

    // Update is called once per frame
    private void Update()
    {

    }

    public override void OnEquip()
    {
        Debug.Log("Equipping pistol");
        base.OnEquip();
        _nextFireTime = 0f;
    }

    #region Firing
    public override void OnPrimaryFireDown()
    {
        TryFire();
    }

    public override void OnPrimaryFireUp()
    {
        // For semi-auto pistol, nothing needed here.
    }

    private void TryFire()
    {
        if (Time.time < _nextFireTime)
            return;

        _nextFireTime = Time.time + (1f / fireRate);

        if (!hasRoundChambered)
        {
            // Click - no ammo in chamber
            PlayDryFire();
            return;
        }

        if (muzzleTransform == null || projectilePrefab == null)
        {
            Debug.LogWarning("PistolWeapon: Missing muzzleTransform or projectilePrefab.");
            return;
        }

        // Spawn projectile
        Quaternion shotRotation = muzzleTransform.rotation;

        // Add optional spread
        if (spreadAngle > 0f)
        {
            shotRotation *= Quaternion.Euler(
                Random.Range(-spreadAngle, spreadAngle),
                Random.Range(-spreadAngle, spreadAngle),
                0f
            );
        }

        GameObject projObj = Object.Instantiate(projectilePrefab, muzzleTransform.position, shotRotation);
        Rigidbody rb = projObj.GetComponent<Rigidbody>();
        rb.linearVelocity = shotRotation * Vector3.forward * bulletSpeed;


        // Consume chambered round
        ConsumeChamberedRound();

        // Play effects
        PlayShootEffects();
    }

    private void ConsumeChamberedRound()
    {
        // We just fired the chambered round. Now we try to feed the next round from the mag.
        if (ammoInMagazine > 0)
        {
            ammoInMagazine--;
            hasRoundChambered = true;
        }
        else
        {
            // Magazine empty, chamber now empty too
            hasRoundChambered = false;
        }
    }

    #endregion

    #region Reload & Slide

    /// <summary>
    /// Called by left-hand reload interaction when inserting a new magazine.
    /// </summary>
    public void InsertMagazineFromLeftHand()
    {
        // Simple rule: only insert a new magazine if the current mag is empty.
        if (ammoInMagazine > 0)
            return;

        if (ammoReserve <= 0)
            return;

        int needed = magazineSize;
        int toLoad = Mathf.Min(needed, ammoReserve);

        ammoInMagazine = toLoad;
        ammoReserve -= toLoad;

        // Play sound
        if (gunAudioSource != null && reloadClip != null)
            gunAudioSource.PlayOneShot(reloadClip);
    }

    /// <summary>
    /// Called by left-hand when interacting with the slide handle.
    /// Puts one round into the chamber if there is ammo in mag.
    /// </summary>
    public void RackSlide()
    {
        if (hasRoundChambered)
        {
            // Already chambered; just play sound/feedback
            PlayRackSlideEffects();
            return;
        }

        if (ammoInMagazine > 0)
        {
            ammoInMagazine--;
            hasRoundChambered = true;

            PlayRackSlideEffects();
        }
        else
        {
            // No ammo to chamber; just dry slide sound if you want
            PlayRackSlideEffects();
        }
    }

    public override void OnReload()
    {
        // Optional: if you want a "single button reload", you can call InsertMagazineFromLeftHand here.
        // For your more realistic two-step reload, you may choose to leave this empty.
        InsertMagazineFromLeftHand();
    }

    #endregion

    #region Magazine Eject / Alt Action

    /// <summary>
    /// Alt action: eject magazine using right thumb button.
    /// </summary>
    public override void OnAltAction()
    {
        EjectMagazine();
    }

    private void EjectMagazine()
    {
        if (ammoInMagazine <= 0)
        {
            // Already empty; you might still want a sound or animation
            if (gunAudioSource != null && magEjectClip != null)
                gunAudioSource.PlayOneShot(magEjectClip);
            return;
        }

        // For simplicity: return leftover rounds to reserve
        ammoReserve += ammoInMagazine;
        ammoInMagazine = 0;

        if (gunAudioSource != null && magEjectClip != null)
            gunAudioSource.PlayOneShot(magEjectClip);

        // Optionally, you can spawn a physical mag object here for visual flair.
    }

    #endregion

    #region Effects & Helpers

    private void PlayShootEffects()
    {
        if (gunAudioSource != null && shootClip != null)
            gunAudioSource.PlayOneShot(shootClip);

        if (_rightInteractor != null)
            _rightInteractor.PulseHaptics(0.7f, 0.1f);
    }

    private void PlayDryFire()
    {
        if (gunAudioSource != null && dryFireClip != null)
            gunAudioSource.PlayOneShot(dryFireClip);

        if (_rightInteractor != null)
            _rightInteractor.PulseHaptics(0.2f, 0.05f);
    }

    private void PlayRackSlideEffects()
    {
        if (gunAudioSource != null && rackSlideClip != null)
            gunAudioSource.PlayOneShot(rackSlideClip);

        if (_rightInteractor != null)
            _rightInteractor.PulseHaptics(0.4f, 0.06f);
    }

    #endregion


    // public virtual void OnUnequip() { }
    // public virtual void OnPrimaryFireDown() { }
    // public virtual void OnPrimaryFireUp() { }
    // public virtual void OnReload() { }
    // public virtual void OnAltAction() { }
}
