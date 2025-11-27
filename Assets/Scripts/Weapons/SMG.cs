using UnityEngine;

/// <summary>
/// SMG - Full-auto submachine gun with detachable magazine.
/// Charging handle does NOT lock back when empty (only internal bolt does).
/// Post-reload: Pull charging handle fully back and release to chamber first round.
/// </summary>
public class SMG : FullAutoGun
{
    public override void OnEquip()
    {
        Debug.Log("Equipping SMG");
        base.OnEquip();
        // pullableLocksBack = false is set by FullAutoGun base class
    }
}
