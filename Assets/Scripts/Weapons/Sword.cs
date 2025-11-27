using UnityEngine;

public class Sword : MeleeWeapon
{
    public override void OnEquip()
    {
        Debug.Log("Equipping sword");
        base.OnEquip();
    }
}
