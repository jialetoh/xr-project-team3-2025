using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
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
        if (leftRayInteractor != null)
            leftRayInteractor.SetActive(false);
        if (rightRayInteractor != null)
            rightRayInteractor.SetActive(false);
        Time.timeScale = 1f;
    }

    private void Update()
    {

        if (GameOverManager.IsGameOver)
            return;

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

        _inputSuppressedUntilRealtime = Time.realtimeSinceStartup + inputSuppressionDuration;

        OnPauseMenuStateChanged?.Invoke(false);
    }

    public void Resume(InputAction.CallbackContext context)
    {
        if (context.performed)
            Resume();
    }

    private void EnableRayInteractors(bool menuIsOpen)
    {
        if (leftRayInteractor != null)
            leftRayInteractor.SetActive(false);

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
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    public void Exit(InputAction.CallbackContext context)
    {
        if (context.performed)
            Exit();
    }
}
