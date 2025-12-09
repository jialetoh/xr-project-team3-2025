using UnityEngine;

public class Blunt : MeleeWeapon
{
    // Blunt weapon inherits swing and impact sounds from MeleeWeapon
    // No drag sound - OnTriggerStay does nothing (base implementation)
}
