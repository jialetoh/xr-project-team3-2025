using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Generic pullable interactable for slides, charging handles, pump actions, etc.
/// Constrained to Z-axis movement with configurable limits.
/// </summary>
public class PullableInteractable : MonoBehaviour
{
    [Header("Visual")]
    public Renderer visualRenderer;

    [Header("Pull Settings")]
    public Transform pullTransform;              // The visual object to move
    public Vector3 restLocalPosition;            // Local position at rest
    public Vector3 pulledLocalPosition;          // Local position when fully pulled
    public float pullThreshold = 0.9f;           // 0-1, how far to pull to trigger action

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip grabClip;
    public AudioClip releaseClip;

    [Header("Filtered Movement")]
    [Tooltip("Reference to FilteredTransform for smooth pull tracking. Uses filtered position for smoother input.")]
    public Transform filteredTransform;

    [Header("Events")]
    public UnityEvent onPullComplete;            // Fired when pulled past threshold and released

    private GunWeapon _parentGun;
    private Transform _grabber;
    private Vector3 _grabStartHandPosition;
    private float _currentPullAmount = 0f;
    private bool _isBeingPulled = false;

    private void Awake()
    {
        _parentGun = GetComponentInParent<GunWeapon>();

        if (pullTransform == null)
            pullTransform = transform;

        if (visualRenderer == null)
            visualRenderer = GetComponent<Renderer>();

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

        // Use filtered transform for initial position if available
        Transform trackTarget = filteredTransform != null ? filteredTransform : grabber;
        _grabStartHandPosition = trackTarget.position;
        _isBeingPulled = true;

        if (audioSource != null && grabClip != null)
            audioSource.PlayOneShot(grabClip);
    }

    public void OnRelease()
    {
        if (!_isBeingPulled) return;

        // Check if pulled far enough to trigger action
        if (_currentPullAmount >= pullThreshold)
        {
            onPullComplete?.Invoke();
            NotifyParentGun();
        }

        // Snap back to rest position
        pullTransform.localPosition = restLocalPosition;
        _currentPullAmount = 0f;
        _isBeingPulled = false;

        if (audioSource != null && releaseClip != null)
            audioSource.PlayOneShot(releaseClip);

        _grabber = null;
    }

    public void OnGrabUpdate(Transform grabber)
    {
        if (!_isBeingPulled || _grabber == null) return;

        // Use filtered transform for smoother tracking if available
        Transform trackTarget = filteredTransform != null ? filteredTransform : grabber;

        // Calculate hand movement delta
        Vector3 handDelta = trackTarget.position - _grabStartHandPosition;

        // Project onto pull direction (backward relative to gun)
        Transform gunTransform = _parentGun != null ? _parentGun.transform : transform.parent;
        Vector3 pullDirection = -gunTransform.forward;
        float pullDistance = Vector3.Dot(handDelta, pullDirection);

        // Calculate pull amount (0-1)
        float maxPullDistance = Vector3.Distance(restLocalPosition, pulledLocalPosition);
        _currentPullAmount = Mathf.Clamp01(pullDistance / maxPullDistance);

        // Update visual position (constrained to defined axis)
        pullTransform.localPosition = Vector3.Lerp(restLocalPosition, pulledLocalPosition, _currentPullAmount);
    }

    private void NotifyParentGun()
    {
        if (_parentGun == null) return;

        // Call base class RackSlide - handles all gun types uniformly
        _parentGun.RackSlide();
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
