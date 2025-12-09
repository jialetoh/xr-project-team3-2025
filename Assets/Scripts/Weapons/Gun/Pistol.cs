using UnityEngine;

public class Pistol : SemiAutoGun
{
    public override void OnEquip()
    {
        Debug.Log("Equipping pistol");
        base.OnEquip();

        // Pistol has a pullable slide that locks back when empty
        pullableLocksBack = true;
    }
}
