using UnityEngine;
/// <summary>
/// Base class for SCAR and SMG
/// Continues firing while trigger is held. Uses base class chamber/slide mechanics.
/// For rifles, the charging handle does NOT lock back - only the internal bolt locks.
/// </summary>
public abstract class FullAutoGun : GunWeapon
{
    protected bool _isFiring = false;

    protected virtual void Update()
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

        // Rifles: charging handle does NOT lock back, only internal bolt does
        pullableLocksBack = false;
    }

    public override void OnUnequip()
    {
        base.OnUnequip();
        _isFiring = false;
    }

    public override void OnPrimaryFireDown()
    {
        _isFiring = true;
        // Fire immediately if we can
        if (Time.time >= _nextFireTime)
        {
            TryFire();
        }
    }

    public override void OnPrimaryFireUp()
    {
        _isFiring = false;
    }

    protected virtual void TryFire()
    {
        if (Time.time < _nextFireTime)
            return;

        _nextFireTime = Time.time + (1f / fireRate);

        if (!hasRoundChambered)
        {
            PlayDryFire();
            _isFiring = false;
            return;
        }

        if (muzzleTransform == null || projectilePrefab == null)
        {
            Debug.LogWarning($"{GetType().Name}: Missing muzzleTransform or projectilePrefab.");
            return;
        }

        // Fire the shot
        FireShot();

        // Consume chambered round and try to chamber next
        ConsumeChamberedRound();

        PlayShootEffects();
    }

    protected virtual void FireShot()
    {
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
        Rigidbody rb = projObj.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = shotRotation * Vector3.forward * bulletSpeed;
        }
    }
}
