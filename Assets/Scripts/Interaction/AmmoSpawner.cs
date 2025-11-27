using UnityEngine;

/// <summary>
/// This script spawns ammo at the transform of the GameObject it's attached to.
/// </summary>
public class AmmoSpawner : MonoBehaviour
{
    public Transform filteredTransform;
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
        _currentAmmoInstance = Instantiate(gun.ammoPrefab, transform.position, transform.rotation);

        if (_currentAmmoInstance.TryGetComponent<AmmoInteractable>(out var ammoInteractable))
        {
            ammoInteractable.spawnPoint = transform;
            ammoInteractable.filteredTransform = filteredTransform;
            ammoInteractable.Initialize(gun, gun.ammoInsertPoint);
        }

        Debug.Log("AmmoSpawner: Spawned ammo for " + gun.weaponName);
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
