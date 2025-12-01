using UnityEngine;

public class FireStaff : MagicWeapon
{
    public enum StaffMode
    {
        Staff,
        Bow
    }

    [Header("Projectile Prefabs")]
    public GameObject singleFireballPrefab;
    public GameObject aoeFireballPrefab;
    public GameObject fireDartPrefab;
    public GameObject fireArrowPrefab;

    [Header("Projectile Speeds")]
    public float singleFireballSpeed = 30f;
    public float aoeFireballSpeed = 20f;
    public float fireDartSpeed = 50f;
    public float fireArrowSpeed = 40f;

    [Header("Charging")]
    public float maxChargeTime = 2f;
    public float chargeMultiplier = 2f;  // Max damage/size multiplier at full charge

    [Header("Audio")]
    public AudioClip castClip;
    public AudioClip chargeClip;
    public AudioClip releaseClip;
    public AudioClip modeChangeClip;

    [Header("Trajectory Preview")]
    public TrajectoryPreview trajectoryPreview;
    public LayerMask groundLayer;
    public float defaultExplosionRadius = 5f;

    // State
    private StaffMode _currentMode = StaffMode.Staff;
    private bool _leftHandModifierHeld = false;    // PrimaryHandTrigger - triples projectiles
    private bool _rightHandModifierHeld = false;   // SecondaryHandTrigger - switches to Bow mode
    private bool _isChargingAOE = false;
    private float _chargeStartTime;
    private bool _isDrawingArrow = false;

    public override void OnEquip()
    {
        Debug.Log("Equipping Fire Staff");
        base.OnEquip();
        _currentMode = StaffMode.Staff;
        ResetInputState();
    }

    public override void OnUnequip()
    {
        base.OnUnequip();
        ResetInputState();
    }

    public override void ResetInputState()
    {
        _leftHandModifierHeld = false;
        _rightHandModifierHeld = false;
        _isChargingAOE = false;
        _isDrawingArrow = false;

        // Hide trajectory preview
        if (trajectoryPreview != null)
            trajectoryPreview.Hide();
    }

    private void Update()
    {
        // Update mode based on right hand modifier
        StaffMode newMode = _rightHandModifierHeld ? StaffMode.Bow : StaffMode.Staff;
        if (newMode != _currentMode)
        {
            _currentMode = newMode;
            OnModeChanged();
        }

        // Update trajectory preview while charging AOE
        if (_isChargingAOE && trajectoryPreview != null && rightHandCastPoint != null)
        {
            UpdateTrajectoryPreview();
        }
    }

    private void UpdateTrajectoryPreview()
    {
        Vector3 direction = rightHandCastPoint.forward;
        Vector3 initialVelocity = direction.normalized * aoeFireballSpeed;

        // Get explosion radius from prefab or use default
        float explosionRadius = defaultExplosionRadius;
        if (aoeFireballPrefab != null)
        {
            AOEFireball aoe = aoeFireballPrefab.GetComponent<AOEFireball>();
            if (aoe != null)
            {
                explosionRadius = aoe.explosionRadius;
            }
        }

        trajectoryPreview.UpdateTrajectory(
            rightHandCastPoint.position,
            initialVelocity,
            explosionRadius,
            groundLayer
        );
    }

    private void OnModeChanged()
    {
        Debug.Log($"Fire Staff mode changed to: {_currentMode}");
        if (magicAudioSource != null && modeChangeClip != null)
            magicAudioSource.PlayOneShot(modeChangeClip);

        // Reset any ongoing actions when switching modes
        _isChargingAOE = false;
        _isDrawingArrow = false;
    }

    #region Input Handlers

    // Left Index Trigger (PrimaryIndexTrigger)
    public override void OnLeftIndexDown()
    {
        if (_currentMode == StaffMode.Staff)
        {
            // Fire single-target fireball(s) from left hand
            int count = _leftHandModifierHeld ? 3 : 1;
            FireSingleTargetFireballs(count);
        }
        else // Bow mode
        {
            // Start drawing fire arrow
            _isDrawingArrow = true;
            Debug.Log("Drawing fire arrow...");
        }
    }

    public override void OnLeftIndexUp()
    {
        if (_currentMode == StaffMode.Bow && _isDrawingArrow)
        {
            // Release fire arrow(s)
            int count = _leftHandModifierHeld ? 3 : 1;
            FireArrows(count);
            _isDrawingArrow = false;
        }
    }

    // Right Index Trigger (SecondaryIndexTrigger)
    public override void OnRightIndexDown()
    {
        if (_currentMode == StaffMode.Staff)
        {
            // Start charging AOE fireball
            _isChargingAOE = true;
            _chargeStartTime = Time.time;
            Debug.Log("Charging AOE fireball...");

            // Show trajectory preview
            if (trajectoryPreview != null)
                trajectoryPreview.Show();
        }
        else // Bow mode
        {
            // Fire darts
            FireDarts();
        }
    }

    public override void OnRightIndexUp()
    {
        if (_currentMode == StaffMode.Staff && _isChargingAOE)
        {
            // Hide trajectory preview
            if (trajectoryPreview != null)
                trajectoryPreview.Hide();

            // Release AOE fireball(s)
            float chargeTime = Mathf.Min(Time.time - _chargeStartTime, maxChargeTime);
            float chargePercent = chargeTime / maxChargeTime;
            int count = _leftHandModifierHeld ? 3 : 1;
            FireAOEFireballs(count, chargePercent);
            _isChargingAOE = false;
        }
    }

    // Left Hand Trigger (PrimaryHandTrigger) - Modifier for triple projectiles
    public override void OnLeftHandDown()
    {
        _leftHandModifierHeld = true;
    }

    public override void OnLeftHandUp()
    {
        _leftHandModifierHeld = false;
    }

    // Right Hand Trigger (SecondaryHandTrigger) - Switches to Bow mode
    public override void OnRightHandDown()
    {
        _rightHandModifierHeld = true;
    }

    public override void OnRightHandUp()
    {
        _rightHandModifierHeld = false;
    }

    #endregion

    #region Projectile Spawning

    private void FireSingleTargetFireballs(int count)
    {
        if (singleFireballPrefab == null || leftHandCastPoint == null)
        {
            Debug.LogWarning("FireStaff: Missing singleFireballPrefab or leftHandCastPoint.");
            return;
        }

        Vector3 direction = leftHandCastPoint.forward;

        if (count == 1)
        {
            SpawnProjectile(singleFireballPrefab, leftHandCastPoint, direction, singleFireballSpeed);
        }
        else
        {
            SpawnMultipleProjectiles(singleFireballPrefab, leftHandCastPoint, direction, singleFireballSpeed, count, 10f);
        }

        PlayCastEffects();
        Debug.Log($"Fired {count} single-target fireball(s)");
    }

    private void FireAOEFireballs(int count, float chargePercent)
    {
        if (aoeFireballPrefab == null || rightHandCastPoint == null)
        {
            Debug.LogWarning("FireStaff: Missing aoeFireballPrefab or rightHandCastPoint.");
            return;
        }

        Vector3 direction = rightHandCastPoint.forward;

        if (count == 1)
        {
            GameObject proj = Instantiate(aoeFireballPrefab, rightHandCastPoint.position, Quaternion.LookRotation(direction));

            // Apply charge to projectile
            Projectile projectile = proj.GetComponent<Projectile>();
            if (projectile != null)
            {
                float multiplier = 1f + (chargeMultiplier - 1f) * chargePercent;
                projectile.damage = Mathf.RoundToInt(projectile.damage * multiplier);
            }

            Rigidbody rb = proj.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.linearVelocity = direction.normalized * aoeFireballSpeed;
            }
        }
        else
        {
            for (int i = 0; i < count; i++)
            {
                Vector3 spreadDir = Quaternion.Euler(
                    Random.Range(-15f, 15f),
                    Random.Range(-15f, 15f),
                    0f
                ) * direction;

                GameObject proj = Instantiate(aoeFireballPrefab, rightHandCastPoint.position, Quaternion.LookRotation(spreadDir));

                Projectile projectile = proj.GetComponent<Projectile>();
                if (projectile != null)
                {
                    float multiplier = 1f + (chargeMultiplier - 1f) * chargePercent;
                    projectile.damage = Mathf.RoundToInt(projectile.damage * multiplier);
                }

                Rigidbody rb = proj.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.linearVelocity = spreadDir.normalized * aoeFireballSpeed;
                }
            }
        }

        PlayReleaseEffects();
        Debug.Log($"Fired {count} AOE fireball(s) at {chargePercent * 100:F0}% charge");
    }

    private void FireDarts()
    {
        if (fireDartPrefab == null || rightHandCastPoint == null)
        {
            Debug.LogWarning("FireStaff: Missing fireDartPrefab or rightHandCastPoint.");
            return;
        }

        Vector3 direction = rightHandCastPoint.forward;
        SpawnProjectile(fireDartPrefab, rightHandCastPoint, direction, fireDartSpeed);

        PlayCastEffects();
        Debug.Log("Fired fire dart");
    }

    private void FireArrows(int count)
    {
        if (fireArrowPrefab == null || leftHandCastPoint == null)
        {
            Debug.LogWarning("FireStaff: Missing fireArrowPrefab or leftHandCastPoint.");
            return;
        }

        Vector3 direction = leftHandCastPoint.forward;

        if (count == 1)
        {
            SpawnProjectile(fireArrowPrefab, leftHandCastPoint, direction, fireArrowSpeed);
        }
        else
        {
            SpawnMultipleProjectiles(fireArrowPrefab, leftHandCastPoint, direction, fireArrowSpeed, count, 8f);
        }

        PlayReleaseEffects();
        Debug.Log($"Fired {count} fire arrow(s)");
    }

    #endregion

    #region Effects

    private void PlayCastEffects()
    {
        if (magicAudioSource != null && castClip != null)
            magicAudioSource.PlayOneShot(castClip);

        HapticsManager.Instance?.PulseMagicCastRight();
    }

    private void PlayReleaseEffects()
    {
        if (magicAudioSource != null && releaseClip != null)
            magicAudioSource.PlayOneShot(releaseClip);

        HapticsManager.Instance?.PulseMagicReleaseRight();
    }

    #endregion
}
