using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class WeaponState
{
    public int ammoInMagazine;
    public int ammoReserve;
    public bool hasRoundChambered;
    public bool isBoltLocked;
    public bool magazineVisualActive;

    public WeaponState() { }

    public WeaponState(GunWeapon gun)
    {
        ammoInMagazine = gun.ammoInMagazine;
        ammoReserve = gun.ammoReserve;
        hasRoundChambered = gun.hasRoundChambered;
        isBoltLocked = gun.isBoltLocked;
        magazineVisualActive = gun.magazineVisual != null && gun.magazineVisual.activeInHierarchy;
    }

    public virtual void ApplyTo(GunWeapon gun)
    {
        gun.ammoInMagazine = ammoInMagazine;
        gun.ammoReserve = ammoReserve;
        gun.hasRoundChambered = hasRoundChambered;
        gun.isBoltLocked = isBoltLocked;

        if (gun.magazineVisual != null)
            gun.magazineVisual.SetActive(magazineVisualActive);

        gun.UpdatePullableVisualState();
    }
}

[System.Serializable]
public class ShotgunState : WeaponState
{
    public int shellsInTube;

    public ShotgunState() : base() { }

    public ShotgunState(Shotgun shotgun) : base(shotgun)
    {
        shellsInTube = shotgun.GetShellsInTube();
    }

    public override void ApplyTo(GunWeapon gun)
    {
        base.ApplyTo(gun);
        if (gun is Shotgun shotgun)
        {
            shotgun.SetShellsInTube(shellsInTube);
        }
    }
}
