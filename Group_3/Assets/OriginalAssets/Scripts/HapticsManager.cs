using System.Collections;
using UnityEngine;

public class HapticsManager : MonoBehaviour
{
    public static HapticsManager Instance { get; private set; }

    // Haptic presets
    [Header("Preset Amplitudes")]
    public float shootAmplitude = 0.7f;
    public float dryFireAmplitude = 0.2f;
    public float reloadAmplitude = 0.4f;
    public float slideRackAmplitude = 0.4f;
    public float meleeHitAmplitude = 0.5f;
    public float magicCastAmplitude = 0.5f;
    public float magicReleaseAmplitude = 0.7f;
    public float interactAmplitude = 0.3f;
    public float damagedAmplitude = 0.8f;

    [Header("Preset Durations")]
    public float shootDuration = 0.1f;
    public float dryFireDuration = 0.05f;
    public float reloadDuration = 0.1f;
    public float slideRackDuration = 0.06f;
    public float meleeHitDuration = 0.08f;
    public float magicCastDuration = 0.1f;
    public float magicReleaseDuration = 0.15f;
    public float interactDuration = 0.05f;
    public float damagedDuration = 0.1f;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void Pulse(OVRInput.Controller controller, float amplitude, float duration)
    {
        StartCoroutine(PulseCoroutine(controller, amplitude, duration));
    }

    public void PulseLeft(float amplitude, float duration)
    {
        Pulse(OVRInput.Controller.LTouch, amplitude, duration);
    }

    public void PulseRight(float amplitude, float duration)
    {
        Pulse(OVRInput.Controller.RTouch, amplitude, duration);
    }

    public void PulseBoth(float amplitude, float duration)
    {
        PulseLeft(amplitude, duration);
        PulseRight(amplitude, duration);
    }

    private IEnumerator PulseCoroutine(OVRInput.Controller controller, float amplitude, float duration)
    {
        OVRInput.SetControllerVibration(1f, amplitude, controller);
        yield return new WaitForSeconds(duration);
        OVRInput.SetControllerVibration(0f, 0f, controller);
    }

    // Preset haptics for right controller
    public void PulseShootRight()
    {
        PulseRight(shootAmplitude, shootDuration);
    }

    public void PulseDryFireRight()
    {
        PulseRight(dryFireAmplitude, dryFireDuration);
    }

    public void PulseReloadRight()
    {
        PulseRight(reloadAmplitude, reloadDuration);
    }

    public void PulseSlideRackRight()
    {
        PulseRight(slideRackAmplitude, slideRackDuration);
    }

    public void PulseMeleeHitRight()
    {
        PulseRight(meleeHitAmplitude, meleeHitDuration);
    }

    public void PulseMagicCastRight()
    {
        PulseRight(magicCastAmplitude, magicCastDuration);
    }

    public void PulseMagicReleaseRight()
    {
        PulseRight(magicReleaseAmplitude, magicReleaseDuration);
    }


    // Preset haptics for left controller
    public void PulseShootLeft()
    {
        PulseLeft(shootAmplitude, shootDuration);
    }

    public void PulseReloadLeft()
    {
        PulseLeft(reloadAmplitude, reloadDuration);
    }

    public void PulseSlideRackLeft()
    {
        PulseLeft(slideRackAmplitude, slideRackDuration);
    }

    public void PulseMagicCastLeft()
    {
        PulseLeft(magicCastAmplitude, magicCastDuration);
    }

    public void PulseMagicReleaseLeft()
    {
        PulseLeft(magicReleaseAmplitude, magicReleaseDuration);
    }

    public void PulseInteractLeft()
    {
        PulseLeft(interactAmplitude, interactDuration);
    }


    // Preset haptics for both controllers
    public void PulseDamagedBoth()
    {
        PulseBoth(damagedAmplitude, damagedDuration);
    }
}
