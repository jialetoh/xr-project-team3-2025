using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;

public class PauseMenuScript : MonoBehaviour
{
    [Header("UI & Interactors")]
    [SerializeField] private GameObject pauseMenuCanvas;
    [SerializeField] private GameObject leftRayInteractor;
    [SerializeField] private GameObject rightRayInteractor;

    [Header("Input Settings")]
    [SerializeField] private float inputSuppressionDuration = 0.25f;

    // Pause state
    public static bool GameIsPaused { get; private set; } = false;
    
    // Static input suppression
    private static float _inputSuppressedUntilRealtime = 0f;

    // Events
    public static event UnityAction<bool> OnPauseMenuStateChanged;

    private void Start()
    {
        if (pauseMenuCanvas != null)
            pauseMenuCanvas.SetActive(false);

        GameIsPaused = false;
        // Ensure left interactor is always disabled; initialize right interactor off
        if (leftRayInteractor != null)
            leftRayInteractor.SetActive(false);
        if (rightRayInteractor != null)
            rightRayInteractor.SetActive(false);
        Time.timeScale = 1f;
    }

    private void Update()
    {
        if (OVRInput.GetDown(OVRInput.Button.Start))
        {
            if (GameIsPaused) Resume();
            else Pause();
        }
    }

    public void Pause()
    {
        if (pauseMenuCanvas != null)
            pauseMenuCanvas.SetActive(true);

        EnableRayInteractors(true);
        Time.timeScale = 0f;
        GameIsPaused = true;

        OnPauseMenuStateChanged?.Invoke(true);
    }

    public void Resume()
    {
        if (pauseMenuCanvas != null)
            pauseMenuCanvas.SetActive(false);

        EnableRayInteractors(false);
        Time.timeScale = 1f;
        GameIsPaused = false;

        // Suppress input briefly to prevent stacking
        _inputSuppressedUntilRealtime = Time.realtimeSinceStartup + inputSuppressionDuration;

        OnPauseMenuStateChanged?.Invoke(false);
    }

    private void EnableRayInteractors(bool menuIsOpen)
    {
        // Left interactor should remain always disabled
        if (leftRayInteractor != null)
            leftRayInteractor.SetActive(false);

        // Right interactor is enabled only when the menu is open
        if (rightRayInteractor != null)
            rightRayInteractor.SetActive(menuIsOpen);
    }

    public static bool IsInputSuppressed()
    {
        return Time.realtimeSinceStartup < _inputSuppressedUntilRealtime;
    }

    public void Exit()
    {
        Time.timeScale = 1f;
        Application.Quit();
    }
}
