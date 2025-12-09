using UnityEngine;

/// <summary>
/// Generic pullable interactable for slides, charging handles, pump actions, etc.
/// Constrained to Z-axis movement with configurable limits.
/// </summary>
public class PullableInteractable : MonoBehaviour
{
    [Header("Pull Settings")]
    [HideInInspector] public Transform pullTransform;              // The visual object to move
    public Vector3 restLocalPosition;            // Local position at rest
    public Vector3 pulledLocalPosition;          // Local position when fully pulled
    public float pullThreshold = 0.9f;           // 0-1, how far to pull to trigger action

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip pullableGrabbedClip;
    public AudioClip pullablePulledClip;    // Plays when pulled past threshold
    public AudioClip pullableReleasedClip;

    private GunWeapon _parentGun;
    private Transform _grabber;
    private Vector3 _grabStartHandPosition;
    private float _currentPullAmount = 0f;
    private bool _isBeingPulled = false;
    private bool _hasPlayedPulledSound = false;

    private void Awake()
    {
        _parentGun = GetComponentInParent<GunWeapon>();

        pullTransform = transform;

        restLocalPosition = pullTransform.localPosition;
    }

    /// <summary>
    /// Always interactable - just like real life, you can always pull the slide/charging handle.
    /// </summary>
    public bool CanInteract()
    {
        return _parentGun != null;
    }

    public void OnGrab(Transform grabber)
    {
        if (!CanInteract()) return;

        _grabber = grabber;
        _grabStartHandPosition = grabber.position;
        _isBeingPulled = true;

        if (audioSource != null && pullableGrabbedClip != null)
            audioSource.PlayOneShot(pullableGrabbedClip);
    }

    public void OnRelease()
    {
        if (!_isBeingPulled) return;

        // Check if pulled far enough to trigger action
        if (_currentPullAmount >= pullThreshold)
        {
            NotifyParentGunRelease();
        }

        // Play release sound
        if (audioSource != null && pullableReleasedClip != null)
            audioSource.PlayOneShot(pullableReleasedClip);

        // Snap back to rest position
        pullTransform.localPosition = restLocalPosition;
        _currentPullAmount = 0f;
        _isBeingPulled = false;
        _hasPlayedPulledSound = false; // Reset for next pull

        _grabber = null;
    }

    public void OnGrabUpdate(Transform grabber)
    {
        if (!_isBeingPulled || _grabber == null) return;

        // Calculate hand movement delta
        Vector3 handDelta = grabber.position - _grabStartHandPosition;

        // Project onto pull direction (backward relative to gun)
        Transform gunTransform = _parentGun != null ? _parentGun.transform : transform.parent;
        Vector3 pullDirection = -gunTransform.forward;
        float pullDistance = Vector3.Dot(handDelta, pullDirection);

        // Calculate pull amount (0-1)
        float maxPullDistance = Vector3.Distance(restLocalPosition, pulledLocalPosition);
        _currentPullAmount = Mathf.Clamp01(pullDistance / maxPullDistance);

        // Play pulled sound and notify gun when crossing threshold (one-time)
        if (_currentPullAmount >= pullThreshold && !_hasPlayedPulledSound)
        {
            if (audioSource != null && pullablePulledClip != null)
                audioSource.PlayOneShot(pullablePulledClip);

            // Notify gun that pullable was pulled back
            NotifyParentGunPulled();
            _hasPlayedPulledSound = true;
        }

        // Update visual position (constrained to defined axis)
        pullTransform.localPosition = Vector3.Lerp(restLocalPosition, pulledLocalPosition, _currentPullAmount);
    }

    private void NotifyParentGunPulled()
    {
        if (_parentGun == null) return;

        // Called when pullable crosses threshold
        _parentGun.OnPullablePulled();
    }

    private void NotifyParentGunRelease()
    {
        if (_parentGun == null) return;

        // Called when pullable is released
        _parentGun.OnPullableReleased();
    }

    public void LockBack()
    {
        pullTransform.localPosition = pulledLocalPosition;
        _currentPullAmount = 1f;
    }

    public void ReturnToRest()
    {
        pullTransform.localPosition = restLocalPosition;
        _currentPullAmount = 0f;
    }

    public float GetPullAmount() => _currentPullAmount;
    public bool IsBeingPulled() => _isBeingPulled;
}
