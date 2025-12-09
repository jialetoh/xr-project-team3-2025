using UnityEngine;

/// <summary>
/// SCAR - Full-auto assault rifle with detachable magazine.
/// Charging handle does NOT lock back when empty (only internal bolt does).
/// Post-reload: Pull charging handle fully back and release to chamber first round.
/// </summary>
public class SCAR : FullAutoGun
{
    public override void OnEquip()
    {
        Debug.Log("Equipping SCAR");
        base.OnEquip();
        // pullableLocksBack = false is set by FullAutoGun base class
    }
}
