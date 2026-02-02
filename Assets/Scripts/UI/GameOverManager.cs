using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameOverManager : MonoBehaviour
{
    public static GameOverManager Instance { get; private set; }

    [Header("UI References")]
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private Canvas gameOverCanvas; // Reference to the canvas for render mode control
    [SerializeField] private Image redOverlay;
    [SerializeField] private TextMeshProUGUI youDiedText;
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private Button restartButton;
    [SerializeField] private GameObject healthbarUI; // Optional: Healthbar to hide on death
    [SerializeField] private StartMenuRayController rayController; // To enable ray on death screen

    [Header("Settings")]
    [SerializeField] private float overlayFadeSpeed = 1f;
    [SerializeField] private Color overlayColor = new Color(1f, 0f, 0f, 0.7f); // Red with alpha

    private float survivalTime = 0f;
    private bool isGameOver = false;
    private bool isTimerRunning = false;

    // Public property to check if game over state is active
    public static bool IsGameOver { get; private set; } = false;

    private void Awake()
    {

        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        // Ensure canvas renders on top of everything
        if (gameOverCanvas != null)
        {
            gameOverCanvas.sortingOrder = 100; // High value to render on top
        }

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }

        // Wire up restart button
        if (restartButton != null)
        {
            restartButton.onClick.AddListener(RestartGame);
        }

        isTimerRunning = true;
        survivalTime = 0f;
    }

    private void Update()
    {

        if (isTimerRunning && !isGameOver)
        {
            survivalTime += Time.deltaTime;
        }
    }

    public void TriggerGameOver()
    {
        if (isGameOver) return;

        isGameOver = true;
        isTimerRunning = false;
        IsGameOver = true; // Set static property

        // Add strong haptic feedback for death/game over
        HapticsManager.Instance?.PulseDamagedBoth();

        // Stop background music
        BackgroundMusicManager.Instance?.StopMusic();

        StartCoroutine(ShowGameOverScreen());
    }

    private IEnumerator ShowGameOverScreen()
    {
        // Hide healthbar
        if (healthbarUI != null)
        {
            healthbarUI.SetActive(false);
        }

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }

        // Enable left ray for death screen interaction
        if (rayController != null)
        {
            rayController.EnableRayForGameOver();
        }

        if (redOverlay != null)
        {
            Color targetColor = overlayColor;
            Color startColor = new Color(overlayColor.r, overlayColor.g, overlayColor.b, 0f);
            redOverlay.color = startColor;

            float elapsed = 0f;
            while (elapsed < overlayFadeSpeed)
            {
                elapsed += Time.deltaTime;
                redOverlay.color = Color.Lerp(startColor, targetColor, elapsed / overlayFadeSpeed);
                yield return null;
            }
            redOverlay.color = targetColor;
        }


        if (youDiedText != null)
        {
            youDiedText.gameObject.SetActive(true);
        }


        if (timerText != null)
        {
            int minutes = Mathf.FloorToInt(survivalTime / 60f);
            int seconds = Mathf.FloorToInt(survivalTime % 60f);
            timerText.text = $"Time Survived: {minutes:00}:{seconds:00}";
            timerText.gameObject.SetActive(true);
        }

        Time.timeScale = 0f; 
    }

    public void RestartGame()
    {
        // Add haptic feedback for restarting
        HapticsManager.Instance?.PulseUIPressLeft();

        isGameOver = false;
        isTimerRunning = true;
        survivalTime = 0f;
        IsGameOver = false; // Reset static property


        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }

        // Optional: Reload scene or reset player
        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
        Time.timeScale = 1f;
    }

    public float GetSurvivalTime()
    {
        return survivalTime;
    }
}
