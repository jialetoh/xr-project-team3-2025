using System;
using System.Collections.Generic;
using UnityEngine;

public class RightControllerInteractor : MonoBehaviour
{
    [Header("Weapons")]
    public List<Weapon> weaponPrefabs;
    [HideInInspector] public Weapon CurrentWeapon;
    private int currentWeaponIndex = 0;

    [SerializeField] private float switchCooldown = 0.3f;
    [SerializeField] private float thumbstickThreshold = 0.1f;
    private float _switchTimer = 0f;

    public AmmoSpawner ammoSpawner;
    public event Action<Weapon> OnWeaponChanged;

    private readonly Dictionary<int, WeaponState> _weaponStates = new();
    private bool _isFirstEquip = true;

    private void Awake()
    {
        EquipWeapon(currentWeaponIndex);
        _isFirstEquip = false;
    }

    private void EquipWeapon(int index)
    {
        // Save current weapon state before destroying
        if (CurrentWeapon != null)
        {
            // Reset input state on the old weapon (polymorphic - each weapon cleans up its own state)
            CurrentWeapon.ResetInputState();
            CurrentWeapon.OnUnequip();

            // Save state based on weapon type
            if (CurrentWeapon is Shotgun shotgunWeapon)
                _weaponStates[currentWeaponIndex] = new ShotgunState(shotgunWeapon);
            else if (CurrentWeapon is GunWeapon gunWeapon)
                _weaponStates[currentWeaponIndex] = new WeaponState(gunWeapon);

            ammoSpawner.OnWeaponUnequipped();
            Destroy(CurrentWeapon.gameObject);
        }

        // Create new weapon instance
        Weapon prefab = weaponPrefabs[index];
        CurrentWeapon = Instantiate(prefab, transform);

        // Restore saved state if it exists
        if (_weaponStates.ContainsKey(index) && CurrentWeapon is GunWeapon gun)
        {
            _weaponStates[index].ApplyTo(gun);
        }

        CurrentWeapon.OnEquip(!_isFirstEquip);
        ammoSpawner.OnWeaponEquipped(CurrentWeapon);

        // Notify subscribers (LeftControllerInteractor)
        OnWeaponChanged?.Invoke(CurrentWeapon);
    }

    private void Update()
    {
        _switchTimer -= Time.deltaTime;

        HandleWeaponSwitch();
        HandleWeaponInputs();
    }

    private void HandleWeaponSwitch()
    {
        if (_switchTimer <= 0f)
        {
            // Weapon switch is controlled by right thumbstick
            Vector2 stick = OVRInput.Get(OVRInput.Axis2D.SecondaryThumbstick);

            if (stick.y >= thumbstickThreshold)
            {
                // Next weapon
                CycleWeapon(+1);
            }
            else if (stick.y <= -thumbstickThreshold)
            {
                // Previous weapon
                CycleWeapon(-1);
            }
        }
    }

    private void CycleWeapon(int delta)
    {
        Debug.Log("Cycling weapon: " + delta);
        _switchTimer = switchCooldown;
        int count = weaponPrefabs.Count;
        currentWeaponIndex = (currentWeaponIndex + delta + count) % count;
        EquipWeapon(currentWeaponIndex);
    }

    private void HandleWeaponInputs()
    {
        CurrentWeapon.HandleRightControllerInputs();
    }
}
