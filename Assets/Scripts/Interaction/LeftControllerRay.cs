using UnityEngine;

// Handles ray rendering, ray hits, and changes object colour based on ray interaction
public class LeftControllerRay : MonoBehaviour
{
    [Header("Ray Parameters")]
    [SerializeField] private float rayLength = 0.5f;
    [SerializeField] private Color rayColor = Color.blue;
    [SerializeField] private GameObject rayCylinder;
    [SerializeField] private float cylinderScaleFactor = 0.01f;
    [SerializeField] private LayerMask interactableLayer;
    [HideInInspector] public Transform RayOrigin;
    private bool _isRayActive = false;

    [Header("Hit Caching")]
    [SerializeField] private float hitObjectCacheTime = 0.2f;
    private float _hitObjectCacheTimer;
    private GameObject _lastHitObject;
    [HideInInspector] public GameObject CurrentHitObject;

    [Header("Ray Interaction")]
    [SerializeField] private Color NormalColor = Color.white;
    [SerializeField] private Color HoverColor = Color.green;
    [SerializeField] private Color GrabColor = Color.red;
    private bool _isGrabbing = false;


    private void Awake()
    {
        RayOrigin = transform;
    }

    private void OnEnable()
    {
        GunInputHandler.OnGrabPressed += TriggerGrabbing;
        GunInputHandler.OnGrabReleased += TriggerNonGrabbing;
    }

    private void OnDisable()
    {
        GunInputHandler.OnGrabPressed -= TriggerGrabbing;
        GunInputHandler.OnGrabReleased -= TriggerNonGrabbing;
    }

    private void Start()
    {
        SetObjectColor(rayCylinder, rayColor);
        SetRayActive(false);
    }

    private void Update()
    {
        if (!_isRayActive) return;

        if (!_isGrabbing)
        {
            RenderRay();
            HandleRaycast();
        }
    }


    // Called on weapon change, active if weapon has left controller interactions
    public void SetRayActive(bool active)
    {
        // Reset everything on weapon change
        _isGrabbing = false;
        CurrentHitObject = null;
        if (_lastHitObject != null) { SetObjectColor(_lastHitObject, NormalColor); }
        _lastHitObject = null;
        _hitObjectCacheTimer = 0f;

        // Ray visibility
        _isRayActive = active;
        rayCylinder.SetActive(active);
    }

    private Ray ConstructRay()
    {
        return new Ray(RayOrigin.position, RayOrigin.forward);
    }

    private void RenderRay()
    {
        Vector3 cylinderCenter = RayOrigin.position + RayOrigin.forward * rayLength / 2;
        Quaternion cylinderRotation = Quaternion.LookRotation(RayOrigin.forward) * Quaternion.Euler(90, 0, 0);
        rayCylinder.transform.SetPositionAndRotation(cylinderCenter, cylinderRotation);
        rayCylinder.transform.localScale = new Vector3(cylinderScaleFactor, rayLength / 2, cylinderScaleFactor);
    }

    private bool CheckHit(Ray ray)
    {
        if (Physics.Raycast(ray, out RaycastHit hitInfo, rayLength, interactableLayer))
        {
            CurrentHitObject = hitInfo.collider.gameObject;
            Debug.Log("Current Hit Object: " + CurrentHitObject.name);

            // Reset previous object colour
            if (_lastHitObject != null && _lastHitObject != CurrentHitObject)
            {
                SetObjectColor(_lastHitObject, NormalColor);
            }
            // Update current object colour
            SetObjectColor(CurrentHitObject, HoverColor);

            // Debug
            if (_lastHitObject != CurrentHitObject) { Debug.Log("Hit Object: " + CurrentHitObject.name); }

            return true;
        }
        return false;
    }

    private void HandleRaycast()
    {
        Ray ray = ConstructRay();

        if (CheckHit(ray))
        {
            // When the ray hits an object, reset the hit object cache timer and update the last hit object.
            _hitObjectCacheTimer = hitObjectCacheTime;
            _lastHitObject = CurrentHitObject;
        }
        else
        {
            // This part is to cache the last hit object for a short period of time to "remember" the object being pointed at.
            if (_hitObjectCacheTimer > 0.0f)
            {
                // The timer is still running, keep the last hit object as the current hit object.
                _hitObjectCacheTimer -= Time.deltaTime;
                CurrentHitObject = _lastHitObject;
            }
            else
            {
                // The timer has expired, clear the last hit object and current hit object.
                if (_lastHitObject != null)
                {
                    // Reset previous object colour
                    SetObjectColor(_lastHitObject, NormalColor);
                    _lastHitObject = null;
                }
                CurrentHitObject = null;
            }
        }
    }

    public void SetObjectColor(GameObject target, Color color)
    {
        if (target.TryGetComponent<Renderer>(out var renderer))
        {
            renderer.material.color = color;
        }
    }

    // Called when GunInputHandler.OnGrabPressed event fires
    private void TriggerGrabbing()
    {
        if (CurrentHitObject != null)
        {
            Debug.Log("LeftControllerRay: Grabbing...");
            _isGrabbing = true;
            SetObjectColor(CurrentHitObject, GrabColor);
            rayCylinder.SetActive(false);
        }
    }

    // Called when GunInputHandler.OnGrabReleased event fires
    private void TriggerNonGrabbing()
    {
        if (_isGrabbing)
        {
            Debug.Log("LeftControllerRay: Releasing...");
            SetObjectColor(CurrentHitObject, NormalColor);
            CurrentHitObject = null;
            _isGrabbing = false;
            rayCylinder.SetActive(_isRayActive);
        }
    }
}
