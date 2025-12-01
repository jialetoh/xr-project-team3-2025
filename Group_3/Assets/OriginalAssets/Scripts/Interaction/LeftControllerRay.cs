using UnityEngine;

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
    public GameObject CurrentHitObject;
    private GameObject _lastHitObject;

    private void Awake()
    {
        RayOrigin = transform;
    }

    private void Start()
    {
        SetRayColor(rayColor);
        SetRayActive(false);
    }

    private void Update()
    {
        if (!_isRayActive) return;

        RenderRay();
        HandleRaycast();
    }

    public void SetRayActive(bool active)
    {
        _isRayActive = active;
        if (rayCylinder != null)
        {
            rayCylinder.SetActive(active);
        }

        if (!active)
        {
            // Clear hit state when deactivating
            CurrentHitObject = null;
            _lastHitObject = null;
            _hitObjectCacheTimer = 0f;
        }
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

    private void HandleRaycast()
    {
        Ray ray = ConstructRay();

        if (CheckHit(ray))
        {
            // Hit something - reset cache timer and update state
            _hitObjectCacheTimer = hitObjectCacheTime;
            _lastHitObject = CurrentHitObject;
        }
        else
        {
            // No hit - use caching (coyote time) to maintain last hit object
            if (_hitObjectCacheTimer > 0f)
            {
                _hitObjectCacheTimer -= Time.deltaTime;
                CurrentHitObject = _lastHitObject;
            }
            else
            {
                // Cache expired - clear hit state
                if (_lastHitObject != null)
                {
                    _lastHitObject = null;
                }
                CurrentHitObject = null;
            }
        }
    }

    private bool CheckHit(Ray ray)
    {
        if (Physics.Raycast(ray, out RaycastHit hitInfo, rayLength, interactableLayer))
        {
            CurrentHitObject = hitInfo.collider.gameObject;
            return true;
        }
        return false;
    }

    public void SetObjectColor(GameObject target, Color color)
    {
        if (target != null && target.TryGetComponent<Renderer>(out var renderer))
        {
            renderer.material.color = color;
        }
    }

    private void SetRayColor(Color color)
    {
        SetObjectColor(rayCylinder, color);
    }
}
