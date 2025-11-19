using UnityEngine;

public class ARWeapon : Weapon
{
    [Header("References")]
    public Transform muzzleTransform;
    public GameObject projectilePrefab;

    [Header("Ballistics")]
    public float bulletSpeed = 70f;
    public float fireRate = 5f;          // shots per second
    public float spreadAngle = 1.5f;       // small random spread

    [Header("Ammo")]
    public int magazineSize = 12;
    public int ammoInMagazine = 12;
    public int ammoReserve = 36;         // total spare rounds

    [Header("Audio")]
    public AudioSource gunAudioSource;
    public AudioClip shootClip;
    public AudioClip dryFireClip;
    public AudioClip reloadClip;
    public AudioClip rackSlideClip;
    public AudioClip magEjectClip;

    private RightControllerInteractor _rightInteractor;
    private bool _isFiring = false;
    private float _nextShotTime = 0f;


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
        if (_isFiring)
        {
            TryFire();
        }
    }

    public override void OnEquip()
    {
        base.OnEquip();
        _isFiring = false;
        _nextShotTime = 0f;

        ammoInMagazine = Mathf.Clamp(ammoInMagazine, 0, magazineSize);
        ammoReserve = Mathf.Max(ammoReserve, 0);
    }

    public override void OnUnequip()
    {
        base.OnUnequip();
        _isFiring = false;
    }

    public override void OnPrimaryFireDown()
    {
        // Start shooting
        _isFiring = true;
        if (Time.time >= _nextShotTime)
        {
            TryFire();
        }
    }

    public override void OnPrimaryFireUp()
    {
        _isFiring = false;
    }

    private void TryFire()
    {
        if (Time.time < _nextShotTime)
            return;

        // Enforce fire rate
        _nextShotTime = Time.time + (1f / fireRate);

        // No ammo in mag
        if (ammoInMagazine <= 0)
        {
            {
                PlayDryFire();
                _isFiring = false; // stop trying to fire
            }

            return;
        }

        if (muzzleTransform == null || projectilePrefab == null)
        {
            Debug.LogWarning("ARWeapon: Missing muzzleTransform or projectilePrefab.");
            return;
        }

        // Spawn projectile with random spread
        Quaternion shotRotation = muzzleTransform.rotation;

        if (spreadAngle > 0f)
        {
            float spreadX = Random.Range(-spreadAngle, spreadAngle);
            float spreadY = Random.Range(-spreadAngle, spreadAngle);
            shotRotation *= Quaternion.Euler(spreadX, spreadY, 0f);
        }

        GameObject projObj = Instantiate(projectilePrefab, muzzleTransform.position, shotRotation);

        Rigidbody rb = projObj.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = shotRotation * Vector3.forward * bulletSpeed;
        }

        ammoInMagazine = Mathf.Max(0, ammoInMagazine - 1);

        PlayShootEffects();
    }

    private void PlayShootEffects()
    {
        gunAudioSource.PlayOneShot(shootClip);
        _rightInteractor.PulseHaptics(0.5f, 0.07f);
    }

    private void PlayDryFire()
    {
        gunAudioSource.PlayOneShot(dryFireClip);
        _rightInteractor.PulseHaptics(0.2f, 0.05f);
    }


    // public virtual void OnAltAction() { }
}
