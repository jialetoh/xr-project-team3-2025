using UnityEngine;

public static class MagicInputHandler
{
    public static void HandleRightControllerInputs(MagicWeapon magic)
    {
        // Right Index Trigger
        if (OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.RTouch))
        {
            magic.OnRightIndexDown();
        }
        if (OVRInput.GetUp(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.RTouch))
        {
            magic.OnRightIndexUp();
        }

        // Right Hand Trigger (grip)
        if (OVRInput.GetDown(OVRInput.Button.PrimaryHandTrigger, OVRInput.Controller.RTouch))
        {
            magic.OnRightHandDown();
        }
        if (OVRInput.GetUp(OVRInput.Button.PrimaryHandTrigger, OVRInput.Controller.RTouch))
        {
            magic.OnRightHandUp();
        }
    }

    public static void HandleLeftControllerInputs(MagicWeapon magic, LeftControllerRay ray)
    {

        // Left Index Trigger
        if (OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.LTouch))
        {
            magic.OnLeftIndexDown();
        }
        if (OVRInput.GetUp(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.LTouch))
        {
            magic.OnLeftIndexUp();
        }

        // Left Hand Trigger (grip)
        if (OVRInput.GetDown(OVRInput.Button.PrimaryHandTrigger, OVRInput.Controller.LTouch))
        {
            magic.OnLeftHandDown();
        }
        if (OVRInput.GetUp(OVRInput.Button.PrimaryHandTrigger, OVRInput.Controller.LTouch))
        {
            magic.OnLeftHandUp();
        }
    }
}
