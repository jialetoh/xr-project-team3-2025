using UnityEngine;


public class RightHandTeleportDisabler : MonoBehaviour
{
    [Header("Teleport Setup")]
    [SerializeField] private UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation.TeleportationProvider rightTeleportProvider;
    [SerializeField] private GameObject rightTeleportRay; // Optional, the visual ray
    [SerializeField] private bool disableRayWhenPaused = true;

    private void OnEnable()
    {
        PauseMenuScript.OnPauseMenuStateChanged += OnPauseMenuChanged;
    }

    private void OnDisable()
    {
        PauseMenuScript.OnPauseMenuStateChanged -= OnPauseMenuChanged;
    }

    private void OnPauseMenuChanged(bool isPaused)
    {
        if (rightTeleportProvider != null)
            rightTeleportProvider.enabled = !isPaused;

        if (rightTeleportRay != null && disableRayWhenPaused)
            rightTeleportRay.SetActive(!isPaused);
    }
}
