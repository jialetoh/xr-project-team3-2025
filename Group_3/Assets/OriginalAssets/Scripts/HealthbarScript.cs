using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class HealthbarScript : MonoBehaviour
{
    [SerializeField] private Image _healthbarSprite;
    [SerializeField] private float maxhealth = 100f;
    [SerializeField] private float damageAmount = 10f; // Fixed damage amount per hit
    [SerializeField] private float regenDelay = 7f; 
    [SerializeField] private float regenRate = 5f; 
    private float _currentHealth;
    private float _timeSinceLastDamage;
    private bool _isRegenerating;

    private void Start()
    {
        _currentHealth = maxhealth;
        _timeSinceLastDamage = 0f;
        _isRegenerating = false;
        UpdateHealthbar();
    }

    private void Update()
    {
        // Check for Y button press on left controller (Primary button)
        if (OVRInput.GetDown(OVRInput.Button.Four))
        {
            TakeDamage(damageAmount);
        }

        // Handle health regeneration
        if (_currentHealth < maxhealth)
        {
            _timeSinceLastDamage += Time.deltaTime;

            if (_timeSinceLastDamage >= regenDelay)
            {
                if (!_isRegenerating)
                {
                    _isRegenerating = true;
                }

                RegenerateHealth();
            }
        }
    }

    public void TakeDamage(float damage)
    {
        _currentHealth -= damage;
        _currentHealth = Mathf.Clamp(_currentHealth, 0f, maxhealth);
        _timeSinceLastDamage = 0f; // Reset timer when damage is taken
        _isRegenerating = false;
        UpdateHealthbar();
    }

    private void RegenerateHealth()
    {
        _currentHealth += regenRate * Time.deltaTime;
        _currentHealth = Mathf.Clamp(_currentHealth, 0f, maxhealth);
        
        if (_currentHealth >= maxhealth)
        {
            _isRegenerating = false;
        }
        
        UpdateHealthbar();
    }

    public void UpdateHealthbar(float healthPercentage)
    {
        _currentHealth = healthPercentage * maxhealth;
        _timeSinceLastDamage = 0f; // Reset timer when health is manually updated
        _isRegenerating = false;
        UpdateHealthbar();
    }

    private void UpdateHealthbar()
    {
        _healthbarSprite.fillAmount = _currentHealth / maxhealth;
    }
}