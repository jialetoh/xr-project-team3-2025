using System.Collections;
using UnityEngine;

public class HapticsManager : MonoBehaviour
{
    public static HapticsManager Instance { get; private set; }

    // Haptic presets
    // Amplitudes
    private float shootAmplitude = 0.7f;
    private float dryFireAmplitude = 0.2f;
    private float reloadAmplitude = 0.4f;
    private float slideRackAmplitude = 0.4f;
    private float meleeHitAmplitude = 0.5f;
    private float magicCastAmplitude = 0.5f;
    private float magicReleaseAmplitude = 0.7f;
    private float interactAmplitude = 0.3f;
    private float damagedAmplitude = 0.8f;
    private float uiHoverAmplitude = 0.2f;
    private float uiPressAmplitude = 0.5f;

    // Durations
    private float shootDuration = 0.1f;
    private float dryFireDuration = 0.05f;
    private float reloadDuration = 0.1f;
    private float slideRackDuration = 0.06f;
    private float meleeHitDuration = 0.08f;
    private float magicCastDuration = 0.1f;
    private float magicReleaseDuration = 0.15f;
    private float interactDuration = 0.05f;
    private float damagedDuration = 0.5f;
    private float uiHoverDuration = 0.03f;
    private float uiPressDuration = 0.08f;

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

    // UI haptics - Right controller (primary menu interaction)
    public void PulseUIHoverRight()
    {
        PulseRight(uiHoverAmplitude, uiHoverDuration);
    }

    public void PulseUIPressRight()
    {
        PulseRight(uiPressAmplitude, uiPressDuration);
    }

    // UI haptics - Left controller (secondary menu interaction)
    public void PulseUIHoverLeft()
    {
        PulseLeft(uiHoverAmplitude, uiHoverDuration);
    }

    public void PulseUIPressLeft()
    {
        PulseLeft(uiPressAmplitude, uiPressDuration);
    }
}
