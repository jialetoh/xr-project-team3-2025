using UnityEngine;

/// <summary>
/// Base class for semi-automatic weapons (pistols, semi-auto rifles).
/// Fires once per trigger pull. Uses base class chamber/slide mechanics.
/// Note: pullableLocksBack should be set by subclasses (true for pistols, false for rifles).
/// </summary>
public abstract class SemiAutoGun : GunWeapon
{
    public override void OnEquip()
    {
        base.OnEquip();
        // pullableLocksBack is set by individual subclasses
    }

    public override void OnPrimaryFireDown()
    {
        TryFire();
    }

    public override void OnPrimaryFireUp()
    {
        // Semi-auto: nothing needed on release
    }

    protected virtual void TryFire()
    {
        Debug.Log($"TryFire called - hasRoundChambered: {hasRoundChambered}, ammoInMagazine: {ammoInMagazine}");

        if (Time.time < _nextFireTime)
        {
            Debug.Log("Fire rate cooldown active");
            return;
        }

        _nextFireTime = Time.time + (1f / fireRate);

        if (!hasRoundChambered)
        {
            Debug.Log("No round chambered - dry fire");
            PlayDryFire();
            return;
        }

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


        // Fire the shot
        FireShot();

        // Consume chambered round and try to chamber next
        ConsumeChamberedRound();

        PlayShootEffects();
    }

    protected virtual void FireShot()
    {
        Debug.Log($"FireShot called - muzzle: {muzzleTransform.position}, prefab: {projectilePrefab.name}");

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

        Rigidbody rb = projObj.GetComponent<Rigidbody>();
        if (rb != null)
        {
            Debug.Log($"Rigidbody found, setting velocity to {bulletSpeed}");
            rb.linearVelocity = shotRotation * Vector3.forward * bulletSpeed;
        }
        else
        {
            Debug.LogWarning($"No Rigidbody found on projectile {projObj.name}!");
        }
    }
}
