using UnityEngine;

public abstract class MagicWeapon : Weapon
{
    [Header("References")]
    public Transform leftHandCastPoint;
    public Transform rightHandCastPoint;

    [Header("Audio")]
    public AudioSource magicAudioSource;

    // Input methods - magic weapons handle all controller inputs
    public abstract void OnLeftIndexDown();
    public abstract void OnLeftIndexUp();
    public abstract void OnRightIndexDown();
    public abstract void OnRightIndexUp();
    public abstract void OnLeftHandDown();
    public abstract void OnLeftHandUp();
    public abstract void OnRightHandDown();
    public abstract void OnRightHandUp();

    protected virtual void SpawnProjectile(GameObject prefab, Transform spawnPoint, Vector3 direction, float speed)
    {
        if (prefab == null || spawnPoint == null)
        {
            Debug.LogWarning($"{GetType().Name}: Missing prefab or spawn point.");
            return;
        }

        Quaternion rotation = Quaternion.LookRotation(direction);
        GameObject projObj = Instantiate(prefab, spawnPoint.position, rotation);

        Rigidbody rb = projObj.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = direction.normalized * speed;
        }
    }

    protected virtual void SpawnMultipleProjectiles(GameObject prefab, Transform spawnPoint, Vector3 direction, float speed, int count, float spreadAngle)
    {
        for (int i = 0; i < count; i++)
        {
            Vector3 spreadDir = Quaternion.Euler(
                Random.Range(-spreadAngle, spreadAngle),
                Random.Range(-spreadAngle, spreadAngle),
                0f
            ) * direction;

            SpawnProjectile(prefab, spawnPoint, spreadDir, speed);
        }
    }

    #region Input Handling

    public override void HandleLeftControllerInputs(LeftControllerRay ray)
    {
        MagicInputHandler.HandleLeftControllerInputs(this, ray);
    }

    public override void HandleRightControllerInputs()
    {
        MagicInputHandler.HandleRightControllerInputs(this);
    }

    #endregion
}
