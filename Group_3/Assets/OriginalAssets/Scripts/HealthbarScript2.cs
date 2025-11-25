using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class HealthbarScript2 : MonoBehaviour
{
    [SerializeField] private Image _healthbarSprite;
    [SerializeField] private float maxhealth = 100f;
    [SerializeField] private float damageAmount = 10f;

    [Header("Head Offset (local)")]
    [SerializeField] private Vector3 localOffset = new Vector3(0f, -0.3f, 1.2f); // tweak in Inspector
    [SerializeField] private bool autoFaceForward = true;

    private float _currentHealth;

    private void Start()
    {
        _currentHealth = maxhealth;
        UpdateHealthbar();
        ApplyOffset();
    }

    private void LateUpdate()
    {
        // If you move offset at runtime, keep applying
        ApplyOffset();
        if (autoFaceForward && transform.parent != null)
        {
            // Face same forward as head (yaw only)
            Vector3 e = transform.parent.rotation.eulerAngles;
            transform.rotation = Quaternion.Euler(0f, e.y, 0f);
        }
    }

    private void ApplyOffset()
    {
        // Ensure we are using local space (parent should be CenterEyeAnchor)
        transform.localPosition = localOffset;
    }

    private void Update()
    {
        if (OVRInput.GetDown(OVRInput.Button.Four))
        {
            TakeDamage(damageAmount);
        }
    }

    public void TakeDamage(float damage)
    {
        _currentHealth = Mathf.Clamp(_currentHealth - damage, 0f, maxhealth);
        UpdateHealthbar();
    }

    public void UpdateHealthbar(float healthPercentage)
    {
        _currentHealth = Mathf.Clamp01(healthPercentage) * maxhealth;
        UpdateHealthbar();
    }

    private void UpdateHealthbar()
    {
        _healthbarSprite.fillAmount = _currentHealth / maxhealth;
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (Application.isPlaying == false)
        {
            // Update position in edit mode when you tweak offset
            ApplyOffset();
        }
    }
#endif
}