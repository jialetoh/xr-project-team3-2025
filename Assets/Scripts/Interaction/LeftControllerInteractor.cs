using UnityEngine;

public class LeftControllerInteractor : MonoBehaviour
{
    [SerializeField] private RightControllerInteractor rightController;
    [SerializeField] private LeftControllerRay leftRay;
    private Weapon _currentWeapon;

    private void OnEnable()
    {
        rightController.OnWeaponChanged += HandleWeaponChanged;
    }

    private void HandleWeaponChanged(Weapon newWeapon)
    {
        _currentWeapon = newWeapon;

        // Enable ray for gun weapons only
        leftRay.SetRayActive(newWeapon is GunWeapon);
    }

    private void Update()
    {
        _currentWeapon.HandleLeftControllerInputs(leftRay);
    }

    private void OnDisable()
    {
        rightController.OnWeaponChanged -= HandleWeaponChanged;
    }
}

