using UnityEngine;
using System;

public static class GunInputHandler
{
    // Left hand interaction state
    private static AmmoInteractable _grabbedAmmo;
    private static PullableInteractable _grabbedPullable;

    // Grabbing events (PrimaryIndexTrigger)
    public static event Action OnGrabPressed;
    public static event Action OnGrabReleased;

    public static void HandleRightControllerInputs(GunWeapon gun)
    {
        if (OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.RTouch))
        {
            gun.OnPrimaryFireDown();
        }
        if (OVRInput.GetUp(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.RTouch))
        {
            gun.OnPrimaryFireUp();
        }

        if (OVRInput.GetDown(OVRInput.Button.PrimaryHandTrigger, OVRInput.Controller.RTouch))
        {
            gun.OnAltAction();
        }
        if (OVRInput.GetDown(OVRInput.Button.One, OVRInput.Controller.RTouch))
        {
            gun.OnReload();
            HapticsManager.Instance?.PulseReloadRight();
        }
    }

    public static void ResetState()
    {
        _grabbedAmmo = null;
        _grabbedPullable = null;
    }

    // Called when ammo auto-inserts to force release the grab state
    public static void ForceReleaseGrab()
    {
        OnGrabReleased?.Invoke();
        _grabbedAmmo = null;
        _grabbedPullable = null;
    }

    public static void HandleLeftControllerInputs(GunWeapon gun, LeftControllerRay ray)
    {
        if (gun == null || ray == null) return;

        if (_grabbedAmmo != null || _grabbedPullable != null)
        {
            HandleGrabbedState(ray);
            return;
        }

        HandleHoverAndGrab(gun, ray);
    }

    private static void HandleHoverAndGrab(GunWeapon gun, LeftControllerRay ray)
    {
        GameObject hitObject = ray.CurrentHitObject;

        if (hitObject == null)
        {
            return;
        }

        // Check for interactables
        AmmoInteractable ammo = hitObject.GetComponent<AmmoInteractable>();
        if (ammo == null) ammo = hitObject.GetComponentInParent<AmmoInteractable>();

        PullableInteractable pullable = hitObject.GetComponent<PullableInteractable>();
        if (pullable == null) pullable = hitObject.GetComponentInParent<PullableInteractable>();

        // Check for grab input
        if (OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.LTouch))
        {
            if (ammo != null)
            {
                GrabAmmo(ammo, ray);
            }
            else if (pullable != null && pullable.CanInteract())
            {
                GrabPullable(pullable, ray);
            }
        }
    }

    private static void GrabAmmo(AmmoInteractable ammo, LeftControllerRay ray)
    {
        _grabbedAmmo = ammo;
        _grabbedPullable = null;

        ammo.OnGrab(ray.RayOrigin);
        OnGrabPressed?.Invoke();

        HapticsManager.Instance?.PulseInteractLeft();
    }

    private static void GrabPullable(PullableInteractable pullable, LeftControllerRay ray)
    {
        _grabbedPullable = pullable;
        _grabbedAmmo = null;

        pullable.OnGrab(ray.RayOrigin);
        OnGrabPressed?.Invoke();

        HapticsManager.Instance?.PulseInteractLeft();
    }

    private static void HandleGrabbedState(LeftControllerRay ray)
    {
        if (_grabbedAmmo != null)
        {
            _grabbedAmmo.OnGrabUpdate(ray.RayOrigin);
        }
        else if (_grabbedPullable != null)
        {
            _grabbedPullable.OnGrabUpdate(ray.RayOrigin);
        }

        if (OVRInput.GetUp(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.LTouch))
        {
            ReleaseGrabbedObject(ray);
        }
    }

    private static void ReleaseGrabbedObject(LeftControllerRay ray)
    {
        if (_grabbedAmmo != null)
        {
            _grabbedAmmo.OnRelease();
        }
        else if (_grabbedPullable != null)
        {
            _grabbedPullable.OnRelease();
        }

        OnGrabReleased?.Invoke();
        _grabbedAmmo = null;
        _grabbedPullable = null;
    }
}
