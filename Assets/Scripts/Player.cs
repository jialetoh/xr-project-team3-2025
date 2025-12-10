using UnityEngine;

public class Player : MonoBehaviour, IDamageable
{
    [Header("Player Stats")]
    [Tooltip("The maximum health of the player")]
    public int Health = 300;
    [Tooltip("The delay before health regeneration starts after taking damage")]
    public float RegenDelay = 7f;
    [Tooltip("The amount of health to regenerate per second")]
    public float RegenHealth = 5f;

    [Header("Healthbar")]
    [Tooltip("The healthbar sprite representing the player's health")]
    public UnityEngine.UI.Image _healthbarSprite;

    [Tooltip("The current health of the player")]
    private int CurrHealth = 300;
    [Tooltip("The time elapsed since the player last took damage")]
    private float TimeSinceLastDamage = 0f;
    [Tooltip("Indicates whether the player is currently regenerating health")]
    private bool IsRegenerating = false;

    private void Awake()
    {
        CurrHealth = Health;
    }

    private void Start()
    {
        UpdateHealthbar();
    }

    private void Update()
    {
        // Skip input processing while paused or during the short post-resume suppression window
        if (PauseMenuScript.GameIsPaused || PauseMenuScript.IsInputSuppressed())
            return; // skip input

        // Handle health regeneration
        if (CurrHealth < Health)
        {
            TimeSinceLastDamage += Time.deltaTime;

            if (TimeSinceLastDamage >= RegenDelay)
            {
                if (!IsRegenerating)
                {
                    IsRegenerating = true;
                }

                RegenerateHealth();
            }
        }
    }

    public void TakeDamage(int Damage)
    {

        CurrHealth -= Damage;
        CurrHealth = (int)Mathf.Clamp(CurrHealth, 0f, Health);
        TimeSinceLastDamage = 0f; // Reset timer when damage is taken
        IsRegenerating = false;
        UpdateHealthbar();

        // Trigger game over when health reaches 0
        if (CurrHealth <= 0f && GameOverManager.Instance != null)
        {
            GameOverManager.Instance.TriggerGameOver();
            HideHealthbar();
        }
    }

    public void HideHealthbar()
    {
        _healthbarSprite.gameObject.SetActive(false);
    }

    private void RegenerateHealth()
    {
        CurrHealth += (int)(RegenHealth * Time.deltaTime);
        CurrHealth = (int)Mathf.Clamp(CurrHealth, 0f, Health);
        
        if (CurrHealth >= Health)
        {
            IsRegenerating = false;
        }
        
        UpdateHealthbar();
    }

    public void UpdateHealthbar(float healthPercentage)
    {
        CurrHealth = (int)(healthPercentage * Health);
        TimeSinceLastDamage = 0f; // Reset timer when health is manually updated
        IsRegenerating = false;
        UpdateHealthbar();
    }

    private void UpdateHealthbar()
    {
        _healthbarSprite.fillAmount = (float)CurrHealth / Health;
    }

    public Transform GetTransform()
    {
        return transform;
    }
}