using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StartMenuManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject startMenuPanel;
    [SerializeField] private Button startButton;
    [SerializeField] private Button quitButton;

    [Header("Game Objects to Hide/Show")]
    [SerializeField] private GameObject healthbarUI; 
    [SerializeField] private GameObject rightControllerWeapon; 
    [SerializeField] private GameObject leftControllerInteractor; 

    [Header("Teleport Settings")]
    [SerializeField] private Transform player;
    [SerializeField] private Vector3 arenaPosition = new Vector3(0, 0, 0); 
    [SerializeField] private Vector3 arenaRotation = new Vector3(0, 0, 0); 
    //start location: (12.608, 1.915, 2.85)
    // start rotation: (0, 270, 0)

    [Header("Optional Settings")]
    [SerializeField] private bool pauseGameUntilStart = true;

    private bool gameStarted = false;

    private void Start()
    {
        if (startMenuPanel != null)
        {
            startMenuPanel.SetActive(true);
        }

        if (startButton != null)
        {
            startButton.onClick.AddListener(StartGame);
        }

        if (quitButton != null)
        {
            quitButton.onClick.AddListener(QuitGame);
        }

        // Hide game elements until start
        if (healthbarUI != null)
        {
            healthbarUI.SetActive(false);
        }

        if (rightControllerWeapon != null)
        {
            rightControllerWeapon.SetActive(false);
        }

        if (leftControllerInteractor != null)
        {
            leftControllerInteractor.SetActive(false);
        }

        if (pauseGameUntilStart)
        {
            Time.timeScale = 0f;
        }
    }

    public void StartGame()
    {
        if (gameStarted) return;

        gameStarted = true;

        if (startMenuPanel != null)
        {
            startMenuPanel.SetActive(false);
        }

        // Show game elements
        if (healthbarUI != null)
        {
            healthbarUI.SetActive(true);
        }

        if (rightControllerWeapon != null)
        {
            rightControllerWeapon.SetActive(true);
        }

        if (leftControllerInteractor != null)
        {
            leftControllerInteractor.SetActive(true);
        }

        if (player != null)
        {
            player.position = arenaPosition;
            player.rotation = Quaternion.Euler(arenaRotation);
            Debug.Log($"Player teleported to {arenaPosition}");
        }

        Time.timeScale = 1f;

        Debug.Log("Game Started!");
    }


    public void QuitGame()
    {
        Debug.Log("Quitting game...");
        Application.Quit();

        // For Unity Editor testing
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }
}
