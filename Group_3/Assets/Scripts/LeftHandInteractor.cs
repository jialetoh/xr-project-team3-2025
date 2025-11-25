using UnityEngine;

public class LeftHandInteractor : MonoBehaviour
{
    public Transform wristTransform;
    public RightHandInteractor rightHandInteractor;

    public bool isActivated = false;
    public bool isScaling = false;

    public Vector3 offset;
    public GameObject visualHand;

    public float originalDistance;
    public float savedDistance;
    public Vector3 originalScale;
    public float minScale = 0.2f;
    public float maxScale = 5.0f;
    public float staticScaleRecord = 1.0f;
    public float staticScaleFactor = 1.0f;

    public Vector3 LeftHandPosition => wristTransform.position;
    public Vector3 RightHandPosition => rightHandInteractor.wristTransform.position;

    public GameObject CurrentGrabbingObject => rightHandInteractor.currentHitObject;

    public void SetActivateState(bool state)
    {
        if (state != isActivated)
        {
            isActivated = state;
            if (isActivated)
            {

            }
            else
            {

            }
        }
    }

    public void TriggerGrabbing()
    {
        if (isActivated)
        {
            if (!isScaling)
            {
                Vector3 leftHandPosition = wristTransform.position;
                Vector3 rightHandPosition = rightHandInteractor.wristTransform.position;
                originalScale = rightHandInteractor.currentHitObject.transform.localScale;
                originalDistance = Vector3.Distance(leftHandPosition, rightHandPosition);
                savedDistance = originalDistance;
                isScaling = true;
            }
        }
    }

    public void TriggerNonGrabbing()
    {
        if (isScaling)
        {
            isScaling = false;
            staticScaleFactor = CurrentGrabbingObject.transform.localScale.x / staticScaleRecord;
            originalScale = CurrentGrabbingObject.transform.localScale;
            originalDistance = savedDistance;
        }
    }

    //  Task 6. Scaling
    //  TODO: Update the scale of the object according to the distance between the two hands.
    public void UpdateScaling(float OriginalDistance, float CurrentDistance, Vector3 OriginalScale, float MinScale, float MaxScale, Vector3 CurrentGrabPoint, Vector3 ObjectOffsetAfterManipulation)
    {
        // Calculate new scale based on ratio of current and original distance
        float scaleFactor = CurrentDistance / OriginalDistance;
        scaleFactor = Mathf.Clamp(scaleFactor, MinScale, MaxScale);
        rightHandInteractor.currentHitObject.transform.localScale = OriginalScale * scaleFactor;
        return;
    }

    void Update()
    {
        if (isActivated)
        {
            visualHand.transform.SetPositionAndRotation(wristTransform.position + offset, wristTransform.rotation);
            if (isScaling)
            {
                Vector3 currentGrabPoint = rightHandInteractor.GrabPoint + rightHandInteractor.handOffset;
                Vector3 objectOffsetWithManipulation = (rightHandInteractor.currentHitObject.transform.position - currentGrabPoint) * staticScaleFactor;
                float currentDistance = Vector3.Distance(LeftHandPosition, RightHandPosition);
                UpdateScaling(originalDistance, currentDistance, originalScale, minScale, maxScale, currentGrabPoint, objectOffsetWithManipulation);
                savedDistance = currentDistance;
            }
            else
            {
                Vector3 currentGrabPoint = rightHandInteractor.GrabPoint + rightHandInteractor.handOffset;
                Vector3 objectOffsetWithManipulation = (rightHandInteractor.currentHitObject.transform.position - currentGrabPoint) * staticScaleFactor;
                UpdateScaling(originalDistance, savedDistance, originalScale, minScale, maxScale, currentGrabPoint, objectOffsetWithManipulation);
            }
        }
        else
        {
            if (isScaling)
            {
                isScaling = false;
            }
            visualHand.transform.SetPositionAndRotation(wristTransform.position, wristTransform.rotation);
        }
    }

    public void TriggerRightHandGrabbing()
    {
        if (rightHandInteractor.currentHitObject != null)
        {

            offset = rightHandInteractor.handOffset;
            originalScale = rightHandInteractor.currentHitObject.transform.localScale;
            staticScaleFactor = 1f;
            staticScaleRecord = originalScale.x;
            savedDistance = 1f;
            originalDistance = 1f;
            SetActivateState(true);
        }
    }
    public void TriggerRightHandNonGrabbing()
    {
        offset = Vector3.zero;
        SetActivateState(false);
    }
}
