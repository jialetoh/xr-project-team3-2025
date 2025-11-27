using UnityEngine;

/// <summary>
/// Bullpup Rifle - Semi-auto rifle with detachable magazine.
/// Charging handle does NOT lock back when empty (only internal bolt does).
/// Post-reload: Pull charging handle fully back and release to chamber first round.
/// </summary>
public class BullpupRifle : SemiAutoGun
{
    public override void OnEquip()
    {
        Debug.Log("Equipping Semi-auto Bullpup Rifle");
        base.OnEquip();

        // Rifles: charging handle does NOT lock back, only internal bolt does
        pullableLocksBack = false;
    }
}
