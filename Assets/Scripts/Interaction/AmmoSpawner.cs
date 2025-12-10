using UnityEngine;

// This script spawns ammo for guns, at the transform of the GameObject it's attached to.
public class AmmoSpawner : MonoBehaviour
{
    private GunWeapon _currentGun;
    private GameObject _currentAmmoInstance;

    public void OnWeaponEquipped(Weapon weapon)
    {
        ClearCurrentAmmo();

        _currentGun = weapon as GunWeapon;
        if (_currentGun != null)
        {
            SpawnAmmo(_currentGun);
        }
    }

    private void ClearCurrentAmmo()
    {
        if (_currentAmmoInstance != null)
        {
            Destroy(_currentAmmoInstance);
            _currentAmmoInstance = null;
        }
    }

    public void OnWeaponUnequipped()
    {
        ClearCurrentAmmo();
        _currentGun = null;
    }

    private void SpawnAmmo(GunWeapon gun)
    {
        Debug.Log($"AmmoSpawner: Spawning ammo at position {transform.position}");
        _currentAmmoInstance = Instantiate(gun.grabbableAmmoPrefab, transform.position, transform.rotation);

        if (_currentAmmoInstance.TryGetComponent<AmmoInteractable>(out var ammoInteractable))
        {
            ammoInteractable.spawnPoint = transform;
            ammoInteractable.Initialize(gun, gun.ammoInsertPoint);

            // Subscribe to grab event for auto-respawn
            ammoInteractable.OnGrabbed += HandleAmmoGrabbed;
            Debug.Log($"AmmoSpawner: Ammo spawned successfully at {_currentAmmoInstance.transform.position}");
        }
        else
        {
            Debug.LogWarning("AmmoSpawner: AmmoInteractable component not found on spawned ammo!");
        }
    }

    private void HandleAmmoGrabbed(AmmoInteractable ammo)
    {
        ammo.OnGrabbed -= HandleAmmoGrabbed;

        // Mark that we no longer track this ammo (it's now grabbed and will either insert or be destroyed)
        _currentAmmoInstance = null;

        if (_currentGun != null)
        {
            SpawnAmmo(_currentGun);
        }
    }

    public void RespawnAmmo()
    {
        if (_currentGun != null)
        {
            ClearCurrentAmmo();
            SpawnAmmo(_currentGun);
        }
    }

    private void OnDestroy()
    {
        ClearCurrentAmmo();
    }
}
