using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RightControllerInteractor : MonoBehaviour
{
    // Weapon Manager
    public List<Weapon> weaponPrefabs;
    public int currentWeaponIndex = 0;
    public float switchThreshold = 0.7f;
    public float switchCooldown = 0.3f;
    private Weapon _currentWeapon;
    private float _switchTimer = 0f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        EquipWeapon(currentWeaponIndex);
    }

    // Update is called once per frame
    private void Update()
    {
        _switchTimer -= Time.deltaTime;

        HandleWeaponSwitch();
        HandleWeaponInputs();
    }

    void HandleWeaponInputs()
    {
        bool triggerDown = OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.RTouch);
        bool triggerUp = OVRInput.GetUp(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.RTouch);

        if (triggerDown) _currentWeapon.OnPrimaryFireDown();
        if (triggerUp) _currentWeapon.OnPrimaryFireUp();

        if (OVRInput.GetDown(OVRInput.Button.SecondaryHandTrigger, OVRInput.Controller.RTouch))
        {
            _currentWeapon.OnAltAction();
        }
    }

    void EquipWeapon(int index)
    {
        if (_currentWeapon != null)
        {
            _currentWeapon.OnUnequip();
            Destroy(_currentWeapon.gameObject);
            Debug.Log("Destroying current weapon!");
        }
        Debug.Log("Equipping weapon");
        Weapon prefab = weaponPrefabs[index];
        _currentWeapon = Instantiate(prefab, transform);
        _currentWeapon.OnEquip();
    }

    void HandleWeaponSwitch()
    {
        if (_switchTimer <= 0f)
        {
            Vector2 stick = OVRInput.Get(OVRInput.RawAxis2D.RThumbstick, OVRInput.Controller.RTouch);
            Debug.Log("stick = " + stick);
            if (stick.y >= switchThreshold)
            {
                Debug.Log("Cycling Weapon +1");
                CycleWeapon(+1);
            }
            else if (stick.y <= -switchThreshold)
            {
                Debug.Log("Cycling Weapon -1");
                CycleWeapon(-1);
            }
        }
    }

    void CycleWeapon(int delta)
    {
        Debug.Log("Cycling weapon");
        _switchTimer = switchCooldown;
        int count = weaponPrefabs.Count;
        currentWeaponIndex = (currentWeaponIndex + delta + count) % count;
        EquipWeapon(currentWeaponIndex);
    }

    public void PulseHaptics(float amplitude, float duration)
    {
        StartCoroutine(HapticsCoroutine(amplitude, duration));
    }

    private IEnumerator HapticsCoroutine(float amplitude, float duration)
    {
        OVRInput.SetControllerVibration(1f, amplitude, OVRInput.Controller.RTouch);
        yield return new WaitForSeconds(duration);
        OVRInput.SetControllerVibration(0f, 0f, OVRInput.Controller.RTouch);
    }
}
