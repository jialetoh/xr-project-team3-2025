using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Haptic feedback component that triggers at the exact moment Unity changes button colors.
/// Uses ISelectHandler for hover (when button becomes "highlighted") and IPointerDownHandler for press.
/// </summary>
public class HapticPauseMenu : MonoBehaviour, ISelectHandler, IPointerDownHandler
{
    private Selectable selectable;
    private bool hasTriggeredHover = false;

    private void Awake()
    {
        selectable = GetComponent<Selectable>();
    }

    // Called when the button is selected/highlighted - this is when Unity changes to "Highlighted" color
    public void OnSelect(BaseEventData eventData)
    {
        if (selectable != null && selectable.interactable && !hasTriggeredHover)
        {
            hasTriggeredHover = true;
            if (HapticsManager.Instance != null)
            {
                HapticsManager.Instance.PulseUIHoverLeft();
            }
        }
    }

    // Called when pointer is pressed down - this is when Unity changes to "Pressed" color
    public void OnPointerDown(PointerEventData eventData)
    {
        if (selectable != null && selectable.interactable)
        {
            if (HapticsManager.Instance != null)
            {
                HapticsManager.Instance.PulseUIPressLeft();
            }
        }
    }

    private void OnDisable()
    {
        hasTriggeredHover = false;
    }
}
