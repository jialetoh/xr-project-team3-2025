using UnityEngine;

public abstract class Weapon : MonoBehaviour
{
    [Header("Weapon Equip Audio")]
    public AudioSource weaponAudioSource;
    public AudioClip equipSound;

    public virtual void OnEquip()
    {
        OnEquip(true);
    }

    public virtual void OnEquip(bool playSound)
    {
        if (playSound)
            PlayEquipSound();
    }

    public virtual void OnUnequip() { }

    protected virtual void PlayEquipSound()
    {
        weaponAudioSource.PlayOneShot(equipSound);
    }

    public virtual void HandleLeftControllerInputs(LeftControllerRay ray) { }
    public virtual void HandleRightControllerInputs() { }
    public virtual void ResetInputState() { }
}
