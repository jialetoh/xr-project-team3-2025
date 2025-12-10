using UnityEngine;

// For the ammo interactable at player's left pocket.
// For the player to grab and insert into the target gun.

public class AmmoInteractable : MonoBehaviour
{
    public System.Action<AmmoInteractable> OnGrabbed;

    [Header("Parameters")]
    public Transform spawnPoint;
    public float snapDistance = 0.1f;
    public int ammoCount = 12;

    private bool _isActiveAmmo = true; // Tracks if this is the current active ammo at spawn point    

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip grabClip;
    public AudioClip insertClip;

    private Transform _grabber;
    private Vector3 _grabOffset;
    private GunWeapon _targetGun;
    private Transform _insertPoint;
    private bool _isGrabbed = false;
    private Vector3 _spawnOffset;
    private Quaternion _spawnRotationOffset;

    public void Initialize(GunWeapon gun, Transform insertPoint)
    {
        _targetGun = gun;
        _insertPoint = insertPoint;

        // Store initial offset from spawn point so ammo follows the player
        if (spawnPoint != null)
        {
            _spawnOffset = spawnPoint.InverseTransformPoint(transform.position);
            _spawnRotationOffset = Quaternion.Inverse(spawnPoint.rotation) * transform.rotation;
        }
    }

    private void Update()
    {
        // When not grabbed and still active, follow the spawn point (player's left pocket)
        if (!_isGrabbed && _isActiveAmmo && spawnPoint != null)
        {
            transform.position = spawnPoint.TransformPoint(_spawnOffset);
            transform.rotation = spawnPoint.rotation * _spawnRotationOffset;
        }
    }

    public void OnGrab(Transform grabber)
    {
        _grabber = grabber;
        _grabOffset = transform.position - grabber.position;
        _isGrabbed = true;
        _isActiveAmmo = false; // No longer the active ammo at spawn point

        if (audioSource != null && grabClip != null)
            audioSource.PlayOneShot(grabClip);

        OnGrabbed?.Invoke(this);
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

        // Follow the grabber's transform with the original grab offset maintained
        transform.position = grabber.position + grabber.rotation * _grabOffset;
        transform.rotation = grabber.rotation;

        // Auto-insert if close enough to mag insert point
        if (_insertPoint != null && _targetGun != null)
        {
            float distance = Vector3.Distance(transform.position, _insertPoint.position);
            if (distance <= snapDistance && _targetGun.CanInsertAmmo())
            {
                InsertIntoGun();
            }
        }
    }

    private void InsertIntoGun()
    {
        if (audioSource != null && insertClip != null)
            audioSource.PlayOneShot(insertClip);

        HapticsManager.Instance?.PulseReloadLeft();

        // If this was auto-inserted while grabbed, reset the grab state
        // so the ray reappears and the input handler clears its grabbed reference
        if (_isGrabbed)
        {
            _isGrabbed = false;
            _grabber = null;
            GunInputHandler.ForceReleaseGrab();
        }

        // Pass this GameObject to become the new magazine visual in the gun
        _targetGun.InsertAmmo(ammoCount, gameObject);

        // Disable this script since the magazine is now part of the gun
        enabled = false;
    }

    private void ReturnToSpawn()
    {
        // Destroy this ammo instance since a new one was already spawned when grabbed
        // This prevents ammo from stacking up at the spawn point
        Debug.Log($"AmmoInteractable: Destroying ungrabbed ammo at position {transform.position}");
        Destroy(gameObject);
    }
}
