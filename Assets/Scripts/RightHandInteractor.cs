using UnityEngine;

public class RightHandInteractor : MonoBehaviour
{
    public Transform wristTransform;
    [HideInInspector] public Ray ray;
    public float hitObjectCacheTime = 0.2f;
    private float hitObjectCacheTimer = 0.0f;
    private GameObject lastHitObject = null;
    public GameObject controllingObject = null;
    public GameObject rayCylinder;
    public float rayLength = 2.0f;
    public Transform grabPointTransform;

    public bool isControlling = false;

    public Vector3 handOffset;
    public GameObject visualHand;
    public Vector3 hitPointOffset;

    private Quaternion OriginalObjectRotation;
    private Quaternion OriginalHandRotation;

    [HideInInspector] public GameObject currentHitObject;
    [HideInInspector] public Vector3 hitPoint;

    public Vector3 GrabPoint
    {
        get
        {
            return grabPointTransform.position;
        }
    }

    private void SetColor(Color color, GameObject target)
    {
        if (target.TryGetComponent<Renderer>(out var renderer))
        {
            renderer.material.color = color;
        }
    }

    //  Task 1. Construct the ray
    //  TODO: Implement the function to construct a ray using the wristTransform's position and orientation.
    public Ray ConstructRay(Transform wristTransform)
    {
        return new Ray(wristTransform.position, wristTransform.forward);
    }

    //  Task 2. Render the ray
    //  TODO: Implement the function to render the ray using a cylinder.
    public void RenderRay(Ray ray, float rayLength)
    {
        Vector3 cylinderCenter = ray.origin + ray.direction * rayLength / 2;
        Quaternion cylinderRotation = Quaternion.LookRotation(ray.direction) * Quaternion.Euler(90, 0, 0);
        rayCylinder.transform.SetPositionAndRotation(cylinderCenter, cylinderRotation);

        float scaleFactor = 0.01f;
        rayCylinder.transform.localScale = new Vector3(scaleFactor, rayLength / 2, scaleFactor);
        return;
    }

    //  Task 3. Intersection between the ray and a virtual object
    //  TODO: Check if there exists intersection between the ray and an Interactable virtual object.
    public bool CheckHit(Ray ray, float rayLength)
    {
        // Only hit the Interactable layer
        int layerMask = LayerMask.GetMask("Interactable");

        // Cast a ray with distance rayLength and store inside hitInfo
        if (Physics.Raycast(ray, out RaycastHit hitInfo, rayLength, layerMask))
        {
            // Debug.Log("Hit object: " + hitInfo.collider.gameObject.name + " on layer: " + LayerMask.LayerToName(hitInfo.collider.gameObject.layer));
            // Update currentHitObject and hitPoint
            currentHitObject = hitInfo.collider.gameObject;
            hitPoint = hitInfo.point;
            SetColor(Color.green, currentHitObject);
            return true;
        }

        return false;
    }

    public void TriggerNonGrabbing()
    {
        if (isControlling)
        {
            SetColor(Color.white, currentHitObject);
            currentHitObject = null;
            isControlling = false;
            rayCylinder.SetActive(true);
        }
    }

    //  Task 4. Trigger object selection by closing your right hand
    //  TODO: Implement the functionality that should be called when the hand is starting to grab (closing fist).
    public void TriggerGrabbing()
    {
        if (currentHitObject != null)
        {
            isControlling = true;
            SetColor(Color.red, currentHitObject);
            rayCylinder.SetActive(false);

            // Save original rotation for object and hand
            OriginalObjectRotation = currentHitObject.transform.rotation;
            OriginalHandRotation = wristTransform.rotation;
        }

        return;
    }

    //  Task 5. Manipulate the object by using your right hand (with closed fist)
    //  TODO: Implement the function to calculate the position and orientation of the object being manipulated.
    public Pose UpdateObjectPose(Vector3 CurrentGrabPoint, Vector3 ObjectOffset, Quaternion OriginalObjectRotation, Quaternion OriginalHandRotation, Quaternion CurrentHandRotation)
    {
        Vector3 updatedPosition = CurrentGrabPoint + ObjectOffset;
        Quaternion updatedRotation = CurrentHandRotation * Quaternion.Inverse(OriginalHandRotation) * OriginalObjectRotation;
        return new Pose(updatedPosition, updatedRotation);
    }

    //  Task 6. Scaling
    //  Go to LeftHandInteractor.cs for implementing this task.
    public LeftHandInteractor leftHandInteractor;

    void Start()
    {
        SetColor(Color.blue, rayCylinder);
    }

    void Update()
    {
        if (isControlling)
        {
            Vector3 currentGrabPoint = handOffset + GrabPoint;

            visualHand.transform.SetPositionAndRotation(wristTransform.position + handOffset, wristTransform.rotation);
            Pose objPose = UpdateObjectPose(currentGrabPoint, hitPointOffset, OriginalObjectRotation, OriginalHandRotation, wristTransform.rotation);
            currentHitObject.transform.SetPositionAndRotation(objPose.position, objPose.rotation);
        }
        else
        {
            visualHand.transform.SetPositionAndRotation(wristTransform.position, wristTransform.rotation);
            ray = ConstructRay(wristTransform);
            RenderRay(ray, rayLength);

            if (CheckHit(ray, rayLength))
            {
                // When the ray hits an object, reset the hit object cache timer and update the last hit object.
                hitObjectCacheTimer = hitObjectCacheTime;
                lastHitObject = currentHitObject;
                // Calculate the offset between the wrist and the grab point.
                handOffset = hitPoint - GrabPoint;
                // Calculate the offset between the hit point and the object's center.
                hitPointOffset = currentHitObject.transform.position - hitPoint;
            }
            else
            {
                // This part is to cache the last hit object for a short period of time to "remember" the object being pointed at.
                if (hitObjectCacheTimer > 0.0f)
                {
                    // The timer is still running, keep the last hit object as the current hit object.
                    hitObjectCacheTimer -= Time.deltaTime;
                    currentHitObject = lastHitObject;
                }
                else
                {
                    // The timer has expired, clear the last hit object and current hit object.
                    if (lastHitObject != null)
                    {
                        SetColor(Color.white, lastHitObject);
                        lastHitObject = null;
                        handOffset = Vector3.zero;
                        hitPointOffset = Vector3.zero;
                    }
                    currentHitObject = null;
                    handOffset = Vector3.zero;
                }
            }
            foreach (GameObject obj in GameObject.FindGameObjectsWithTag("Interactable"))
            {
                if (obj != currentHitObject)
                {
                    SetColor(Color.white, obj);
                }
            }
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        if (isControlling && currentHitObject != null)
        {
            Gizmos.DrawLine(wristTransform.position, currentHitObject.transform.position);
        }
        else
        {
            Gizmos.DrawRay(ray.origin, ray.direction * rayLength);

        }

        if (wristTransform == null) return;

        // FORWARD Blue
        Gizmos.color = Color.blue;
        Gizmos.DrawRay(wristTransform.position, wristTransform.forward * 2);
        // UP Green
        Gizmos.color = Color.green;
        Gizmos.DrawRay(wristTransform.position, wristTransform.up * 2);
        // RIGHT Red
        Gizmos.color = Color.red;
        Gizmos.DrawRay(wristTransform.position, wristTransform.right * 2);
    }
}
