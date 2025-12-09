using UnityEngine;

/// <summary>
/// Shotgun with charging handle and shell-by-shell reload.
///
/// Realistic behavior (simplified for VR):
/// - Shells can be inserted into the tube magazine anytime (loading port underneath is always accessible)
/// - After firing, charging handle must be pulled to chamber next round from tube
/// - Charging handle: pull back and release - it snaps forward automatically
/// - Pulling handle when a round is chambered will eject that live round (wasteful but realistic)
/// </summary>
public class Shotgun : SemiAutoGun
{
    [Header("Shotgun Pellets")]
    public int pelletsPerShot = 8;
    public float pelletSpreadAngle = 5f;

    [Header("Shell Reload")]
    public int tubeCapacity = 6;             // Max shells in tube magazine

    [Header("Shotgun Audio")]
    public AudioClip handlePullClip;         // Sound when pulling charging handle back
    public AudioClip handleReleaseClip;      // Sound when handle snaps forward
    public AudioClip shellInsertClip;        // Sound when inserting shell into tube
    public AudioClip shellEjectClip;         // Sound when live round is ejected

    private int _shellsInTube = 0;           // Shells in tube (not counting chamber)

    public override void OnEquip()
    {
        Debug.Log("Equipping shotgun");
        base.OnEquip();

        // Shotgun: charging handle does NOT lock back visually
        pullableLocksBack = false;

        // Initialize tube magazine from ammoInMagazine
        // In shotgun context, ammoInMagazine represents total shells (tube + chamber)
        if (hasRoundChambered && ammoInMagazine > 0)
        {
            _shellsInTube = ammoInMagazine - 1; // One is chambered
        }
        else
        {
            _shellsInTube = ammoInMagazine;
        }
    }

    protected override void FireShot()
    {
        // Fire multiple pellets
        for (int i = 0; i < pelletsPerShot; i++)
        {
            Quaternion shotRotation = muzzleTransform.rotation;
            shotRotation *= Quaternion.Euler(
                Random.Range(-pelletSpreadAngle, pelletSpreadAngle),
                Random.Range(-pelletSpreadAngle, pelletSpreadAngle),
                0f
            );

            GameObject projObj = Instantiate(projectilePrefab, muzzleTransform.position, shotRotation);
            Rigidbody rb = projObj.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.linearVelocity = shotRotation * Vector3.forward * bulletSpeed;
            }
        }
    }

    protected override void ConsumeChamberedRound()
    {
        // After firing, chamber is empty - need to rack handle to load next round
        hasRoundChambered = false;

        Debug.Log($"{GetType().Name}: Chamber empty. Rack charging handle to load next round. Shells in tube: {_shellsInTube}");
    }

    #region Charging Handle

    /// <summary>
    /// Called when player completes a pull on the charging handle.
    /// This is a "pull and release" action - pulling back then releasing chambers a round.
    /// </summary>
    public override void RackSlide()
    {
        // Pull back and release in one action
        PullHandleAndRelease();
    }

    private void PullHandleAndRelease()
    {
        // Play pull sound
        if (weaponAudioSource != null && handlePullClip != null)
            weaponAudioSource.PlayOneShot(handlePullClip);

        // If there's a chambered round, eject it (wasteful but realistic)
        if (hasRoundChambered)
        {
            hasRoundChambered = false;

            if (weaponAudioSource != null && shellEjectClip != null)
                weaponAudioSource.PlayOneShot(shellEjectClip);

            Debug.Log($"{GetType().Name}: Ejected live round from chamber");
        }

        HapticsManager.Instance?.PulseSlideRackRight();

        // Chamber next round from tube (handle snaps forward)
        ChamberFromTube();
    }

    private void ChamberFromTube()
    {
        // Chamber a round from tube if available
        if (_shellsInTube > 0)
        {
            _shellsInTube--;
            hasRoundChambered = true;
            Debug.Log($"{GetType().Name}: Chambered round from tube. Shells remaining: {_shellsInTube}");
        }
        else
        {
            hasRoundChambered = false;
            Debug.Log($"{GetType().Name}: No shells in tube to chamber");
        }

        // Play release/forward sound
        if (weaponAudioSource != null && handleReleaseClip != null)
            weaponAudioSource.PlayOneShot(handleReleaseClip);

        // Handle returns to rest position
        if (pullableInteractable != null)
            pullableInteractable.ReturnToRest();

        isBoltLocked = false;

        // Update ammoInMagazine to reflect current state
        ammoInMagazine = _shellsInTube + (hasRoundChambered ? 1 : 0);
    }

    #endregion

    #region Shell Loading

    /// <summary>
    /// Shells can be inserted into the tube anytime (loading port is always accessible).
    /// </summary>
    public override bool CanInsertAmmo()
    {
        return _shellsInTube < tubeCapacity;
    }

    public override void InsertAmmo(int amount)
    {
        if (_shellsInTube >= tubeCapacity)
        {
            Debug.Log($"{GetType().Name}: Cannot insert shell - tube full");
            return;
        }

        _shellsInTube++;
        ammoInMagazine = _shellsInTube + (hasRoundChambered ? 1 : 0);

        if (weaponAudioSource != null && shellInsertClip != null)
            weaponAudioSource.PlayOneShot(shellInsertClip);

        HapticsManager.Instance?.PulseReloadLeft();

        Debug.Log($"{GetType().Name}: Shell inserted. Shells in tube: {_shellsInTube}/{tubeCapacity}");
    }

    public int GetShellsInTube() => _shellsInTube;

    public void SetShellsInTube(int count)
    {
        _shellsInTube = Mathf.Clamp(count, 0, tubeCapacity);
        ammoInMagazine = _shellsInTube + (hasRoundChambered ? 1 : 0);
    }

    #endregion

    #region Shotgun Shell Ejection

    protected override void SpawnEjectedRound()
    {
        if (projectilePrefab == null || ejectionPort == null)
            return;

        // Spawn shotgun shell (use projectile prefab)
        GameObject ejectedShell = Instantiate(
            projectilePrefab,
            ejectionPort.position,
            ejectionPort.rotation
        );

        // Remove Projectile script
        Projectile projScript = ejectedShell.GetComponent<Projectile>();
        if (projScript != null)
            Destroy(projScript);

        // Apply physics - shotgun ejects more forcefully
        Rigidbody rb = ejectedShell.GetComponent<Rigidbody>();
        if (rb == null)
            rb = ejectedShell.AddComponent<Rigidbody>();

        rb.useGravity = true;

        // Eject to the right
        Vector3 ejectionDirection = transform.right + transform.up * 0.3f;
        rb.linearVelocity = ejectionDirection.normalized * (ejectionForce * 1.5f) + Vector3.up * ejectionUpwardForce;
        rb.angularVelocity = Random.insideUnitSphere * 8f;

        Destroy(ejectedShell, 10f);
    }

    #endregion

    #region Override Magazine Methods (Disabled for Shotgun)

    public override void OnReload()
    {
        // No automatic reload for shotgun - must insert shells manually
        Debug.Log($"{GetType().Name}: Insert shells manually, then rack charging handle");
    }

    public override void OnAltAction()
    {
        RackSlide();
    }

    public override void InsertMagazine()
    {
        // Shotgun doesn't use magazines
    }

    public override void EjectMagazine()
    {
        // Shotgun doesn't use magazines
    }

    #endregion
}
