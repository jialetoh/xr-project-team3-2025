using UnityEngine;

public static class GunInputHandler
{
    private static readonly Color NormalColor = Color.white;
    private static readonly Color HoverColor = Color.green;
    private static readonly Color GrabColor = Color.red;

    // Left hand interaction state
    private static AmmoInteractable _grabbedAmmo;
    private static PullableInteractable _grabbedPullable;
    private static GameObject _currentHoveredObject;
    private static Renderer _currentHoveredRenderer;
    private static Renderer _currentGrabbedRenderer;

    public static void HandleRightControllerInputs(GunWeapon gun)
    {
        if (gun == null) return;

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
        if (_currentGrabbedRenderer != null)
            _currentGrabbedRenderer.material.color = NormalColor;
        if (_currentHoveredRenderer != null)
            _currentHoveredRenderer.material.color = NormalColor;

        _grabbedAmmo = null;
        _grabbedPullable = null;
        _currentHoveredObject = null;
        _currentHoveredRenderer = null;
        _currentGrabbedRenderer = null;
    }

    public static void HandleLeftControllerInputs(GunWeapon gun, LeftControllerRay ray)
    {
        if (gun == null) return;

        if (ray == null)
        {
            HandleFallbackReload(gun);
            return;
        }

        if (_grabbedAmmo != null || _grabbedPullable != null)
        {
            HandleGrabbedState(ray);
            return;
        }

        HandleHoverAndGrab(gun, ray);
    }

    private static void HandleFallbackReload(GunWeapon gun)
    {
        if (OVRInput.GetDown(OVRInput.Button.PrimaryHandTrigger, OVRInput.Controller.LTouch))
        {
            gun.OnReload();
            HapticsManager.Instance?.PulseReloadLeft();
        }
    }

    private static void HandleHoverAndGrab(GunWeapon gun, LeftControllerRay ray)
    {
        GameObject hitObject = ray.CurrentHitObject;

        if (hitObject == null)
        {
            ClearHover(ray);
            return;
        }

        // Check for interactables
        AmmoInteractable ammo = hitObject.GetComponent<AmmoInteractable>();
        if (ammo == null) ammo = hitObject.GetComponentInParent<AmmoInteractable>();

        PullableInteractable pullable = hitObject.GetComponent<PullableInteractable>();
        if (pullable == null) pullable = hitObject.GetComponentInParent<PullableInteractable>();

        // Determine valid interactable
        GameObject interactableObj = null;
        Renderer interactableRenderer = null;

        if (ammo != null)
        {
            interactableObj = ammo.gameObject;
            interactableRenderer = ammo.visualRenderer;
        }
        else if (pullable != null && pullable.CanInteract())
        {
            interactableObj = pullable.gameObject;
            interactableRenderer = pullable.visualRenderer;
        }

        if (interactableObj == null)
        {
            ClearHover(ray);
            return;
        }

        // New hover target
        if (_currentHoveredObject != interactableObj)
        {
            ClearHover(ray);
            _currentHoveredObject = interactableObj;
            _currentHoveredRenderer = interactableRenderer;
            ray.SetObjectColor(_currentHoveredObject, HoverColor);
        }

        // Check for grab input
        if (OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.LTouch))
        {
            if (ammo != null)
            {
                GrabAmmo(ammo, ray);
            }
            else if (pullable != null)
            {
                GrabPullable(pullable, ray);
            }
        }
    }

    private static void ClearHover(LeftControllerRay ray)
    {
        if (_currentHoveredObject != null)
        {
            ray.SetObjectColor(_currentHoveredObject, NormalColor);
            _currentHoveredObject = null;
            _currentHoveredRenderer = null;
        }
    }

    private static void GrabAmmo(AmmoInteractable ammo, LeftControllerRay ray)
    {
        _grabbedAmmo = ammo;
        _grabbedPullable = null;
        _currentGrabbedRenderer = ammo.visualRenderer;

        ammo.OnGrab(ray.RayOrigin);
        ray.SetObjectColor(ammo.gameObject, GrabColor);
        ClearHoverState();

        HapticsManager.Instance?.PulseInteractLeft();
    }

    private static void GrabPullable(PullableInteractable pullable, LeftControllerRay ray)
    {
        _grabbedPullable = pullable;
        _grabbedAmmo = null;
        _currentGrabbedRenderer = pullable.visualRenderer;

        pullable.OnGrab(ray.RayOrigin);
        ray.SetObjectColor(pullable.gameObject, GrabColor);
        ClearHoverState();

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
            ray.SetObjectColor(_grabbedAmmo.gameObject, NormalColor);
        }
        else if (_grabbedPullable != null)
        {
            _grabbedPullable.OnRelease();
            ray.SetObjectColor(_grabbedPullable.gameObject, NormalColor);
        }

        _grabbedAmmo = null;
        _grabbedPullable = null;
        _currentGrabbedRenderer = null;
    }

    private static void ClearHoverState()
    {
        _currentHoveredObject = null;
        _currentHoveredRenderer = null;
    }
}
