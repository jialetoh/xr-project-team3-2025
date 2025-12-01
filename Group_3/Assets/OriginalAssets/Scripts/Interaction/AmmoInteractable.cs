using UnityEngine;

/// <summary>
/// Unified ammo interactable for all gun types (magazines, shells, etc).
/// Can be grabbed and inserted into the target gun.
/// Uses FilteredTransform for smooth movement when grabbed.
/// </summary>
public class AmmoInteractable : MonoBehaviour
{
    [Header("Visual")]
    public Renderer visualRenderer;

    [Header("Ammo Settings")]
    public Transform spawnPoint;
    public float snapDistance = 0.1f;
    public int ammoCount = 12;

    [Header("Filtered Movement")]
    [Tooltip("Reference to FilteredTransform for smooth grab movement. The ammo will follow this filtered position.")]
    public Transform filteredTransform;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip grabClip;
    public AudioClip insertClip;

    private Transform _grabber;
    private Vector3 _grabOffset;
    private GunWeapon _targetGun;
    private Transform _insertPoint;
    private bool _isGrabbed = false;

    private void Awake()
    {
        if (visualRenderer == null)
            visualRenderer = GetComponent<Renderer>();
    }

    public void Initialize(GunWeapon gun, Transform insertPoint)
    {
        _targetGun = gun;
        _insertPoint = insertPoint;
    }

    public void OnGrab(Transform grabber)
    {
        _grabber = grabber;
        _grabOffset = transform.position - grabber.position;
        _isGrabbed = true;

        if (audioSource != null && grabClip != null)
            audioSource.PlayOneShot(grabClip);
    }

    public void OnRelease()
    {
        if (_isGrabbed && _insertPoint != null && _targetGun != null)
        {
            float distance = Vector3.Distance(transform.position, _insertPoint.position);
            if (distance <= snapDistance && _targetGun.CanInsertAmmo())
            {
                InsertIntoGun();
                return;
            }
        }

        _grabber = null;
        _isGrabbed = false;
        ReturnToSpawn();
    }

    public void OnGrabUpdate(Transform grabber)
    {
        if (!_isGrabbed) return;

        // Use filtered transform if available for smoother movement
        Transform followTarget = filteredTransform != null ? filteredTransform : grabber;

        transform.position = followTarget.position + followTarget.rotation * _grabOffset;
        transform.rotation = followTarget.rotation;
    }

    private void InsertIntoGun()
    {
        if (audioSource != null && insertClip != null)
            audioSource.PlayOneShot(insertClip);

        HapticsManager.Instance?.PulseReloadLeft();

        _grabber = null;
        _isGrabbed = false;

        // Pass this GameObject to become the new magazine visual in the gun
        _targetGun.InsertAmmo(ammoCount, gameObject);

        // Disable this script since the magazine is now part of the gun
        enabled = false;
    }

    private void ReturnToSpawn()
    {
        if (spawnPoint != null)
        {
            transform.position = spawnPoint.position;
            transform.rotation = spawnPoint.rotation;
        }
    }
}
