using UnityEngine;

public class Sword : MeleeWeapon
{
    public override void OnEquip()
    {
        Debug.Log("Equipping sword");
        base.OnEquip();
    }

    protected override void OnTriggerStay(Collider other)
    {
        // Only play drag sound for enemy layers
        if ((hittableLayers.value & (1 << other.gameObject.layer)) == 0)
            return;

        // Only play if moving fast enough and drag sound is assigned
        if (_velocity.magnitude < minHitVelocity || dragSound == null || dragAudioSource == null)
            return;

        // Start looping drag sound if not already playing
        if (!_isDragging)
        {
            dragAudioSource.clip = dragSound;
            dragAudioSource.loop = true;
            dragAudioSource.Play();
            _isDragging = true;
        }
    }
}
