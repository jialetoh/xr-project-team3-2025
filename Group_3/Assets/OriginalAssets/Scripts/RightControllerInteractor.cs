using System;
using System.Collections.Generic;
using UnityEngine;

public class RightControllerInteractor : MonoBehaviour
{
    public List<Weapon> weaponPrefabs;
    [HideInInspector] public Weapon CurrentWeapon;
    private int currentWeaponIndex = 0;

    [SerializeField] private float switchCooldown = 0.3f;
    [SerializeField] private float thumbstickThreshold = 0.7f;
    private float _switchTimer = 0f;

    public AmmoSpawner ammoSpawner;
    public event Action<Weapon> OnWeaponChanged;

    private void Awake()
    {
        EquipWeapon(currentWeaponIndex);
    }

    private void EquipWeapon(int index)
    {
        // Unequip first
        if (CurrentWeapon != null)
        {
            // Reset input state on the old weapon (polymorphic - each weapon cleans up its own state)
            CurrentWeapon.ResetInputState();
            CurrentWeapon.OnUnequip();
            ammoSpawner.OnWeaponUnequipped();
            Destroy(CurrentWeapon.gameObject);
        }

        // Equip
        Weapon prefab = weaponPrefabs[index];
        CurrentWeapon = Instantiate(prefab, transform);
        CurrentWeapon.OnEquip();

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
            Vector2 stick = OVRInput.Get(OVRInput.RawAxis2D.RThumbstick, OVRInput.Controller.RTouch);
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
