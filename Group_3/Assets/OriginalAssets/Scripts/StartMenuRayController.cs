using UnityEngine;

public class StartMenuRayController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private LeftControllerRay leftRay;
    
    private void Start()
    {
        
        if (leftRay != null)
        {
            leftRay.SetRayActive(true);
        }
    }

    private void OnEnable()
    {
        
        PauseMenuScript.OnPauseMenuStateChanged += HandlePauseMenuState;
    }

    private void OnDisable()
    {
        
        PauseMenuScript.OnPauseMenuStateChanged -= HandlePauseMenuState;
    }

    private void HandlePauseMenuState(bool paused)
    {
        
        if (leftRay != null && paused)
        {
            leftRay.SetRayActive(true);
        }
    }

    
    public void EnableRayForGameOver()
    {
        if (leftRay != null)
        {
            leftRay.SetRayActive(true);
        }
    }
}
