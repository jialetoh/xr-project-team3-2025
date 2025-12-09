using UnityEngine;

public class Lightsaber : MeleeWeapon
{
    [Header("Lightsaber Hum")]
    public AudioSource humAudioSource;
    public AudioClip humSound;
    public float basePitch = 1.0f;
    public float baseVolume = 0.7f;
    public float maxPitch = 1.5f;
    public float maxVolume = 1.0f;

    private bool _isActivated = false;

    public override void OnEquip()
    {
        base.OnEquip();
        _isActivated = false;
    }

    protected override void Update()
    {
        base.Update();

        // Check for activation input (PrimaryHandTrigger OR PrimaryIndexTrigger)
        bool activationInput = OVRInput.Get(OVRInput.Button.PrimaryHandTrigger, OVRInput.Controller.RTouch) ||
                               OVRInput.Get(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.RTouch);

        if (activationInput && !_isActivated)
        {
            ActivateLightsaber();
        }
        else if (!activationInput && _isActivated)
        {
            DeactivateLightsaber();
        }

        // Modulate hum based on velocity if activated
        if (_isActivated)
        {
            ModulateHum();
        }
    }

    protected override void CheckAndPlaySwingSound()
    {
        // Lightsaber doesn't use discrete swing sounds - hum modulation handles this
        // Override to do nothing
    }

    private void ActivateLightsaber()
    {
        _isActivated = true;

        if (humAudioSource != null && humSound != null)
        {
            humAudioSource.clip = humSound;
            humAudioSource.loop = true;
            humAudioSource.pitch = basePitch;
            humAudioSource.volume = baseVolume;
            humAudioSource.Play();
        }
    }

    private void DeactivateLightsaber()
    {
        _isActivated = false;

        if (humAudioSource != null && humAudioSource.isPlaying)
        {
            humAudioSource.Stop();
        }
    }

    private void ModulateHum()
    {
        if (humAudioSource == null || !humAudioSource.isPlaying)
            return;

        float speed = _velocity.magnitude;

        // Normalize velocity to 0-1 range
        float normalizedVelocity = Mathf.Clamp01(speed / swingMaxVelocity);

        // Modulate pitch and volume based on velocity
        humAudioSource.pitch = Mathf.Lerp(basePitch, maxPitch, normalizedVelocity);
        humAudioSource.volume = Mathf.Lerp(baseVolume, maxVolume, normalizedVelocity);
    }

    protected override void OnTriggerStay(Collider other)
    {
        // Only activate if lightsaber is on
        if (!_isActivated)
            return;

        // Only play drag sound for enemy layers
        if ((hittableLayers.value & (1 << other.gameObject.layer)) == 0)
            return;

        // Only play if moving fast enough and drag sound is assigned
        if (_velocity.magnitude < minHitVelocity || dragSound == null || dragAudioSource == null)
            return;

        // Start looping energy sizzle drag sound if not already playing
        if (!_isDragging)
        {
            dragAudioSource.clip = dragSound;
            dragAudioSource.loop = true;
            dragAudioSource.Play();
            _isDragging = true;
        }
    }

    protected override void OnTriggerEnter(Collider other)
    {
        // Only deal damage if lightsaber is activated
        if (!_isActivated)
            return;

        base.OnTriggerEnter(other);
    }
}
