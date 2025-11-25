using UnityEngine;

public abstract class Weapon : MonoBehaviour
{
    public string weaponName;

    public virtual void OnEquip() { }
    public virtual void OnUnequip() { }
    public virtual void OnPrimaryFireDown() { } // When primary fire button was pressed this frame
    public virtual void OnPrimaryFireUp() { } // When primary fire button was released this frame
    public virtual void OnReload() { }
    public virtual void OnAltAction() { } // Tied to right thumb trigger, for magazine eject
}
