using UnityEngine;

public abstract class Weapon : MonoBehaviour
{
    public string weaponName;

    public virtual void OnEquip() { }
    public virtual void OnUnequip() { }
    public virtual void HandleLeftControllerInputs(LeftControllerRay ray) { }
    public virtual void HandleRightControllerInputs() { }
    public virtual void ResetInputState() { }
}
