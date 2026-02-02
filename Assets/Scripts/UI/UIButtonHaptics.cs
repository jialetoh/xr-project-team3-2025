using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Haptic feedback for UI buttons. Works with Unity Button components on Canvas.
/// ISelectHandler triggers when ray hovers (button highlights).
/// IPointerDownHandler triggers when button is pressed down.
/// </summary>
public class UIButtonHaptics : MonoBehaviour, ISelectHandler, IPointerDownHandler
{
    [Header("Controller Selection")]
    [Tooltip("Which controller triggers haptics")]
    public OVRInput.Controller controller = OVRInput.Controller.LTouch;

    private Selectable selectable;
    private bool hasTriggeredHover = false;

    private void Awake()
    {
        selectable = GetComponent<Selectable>();
    }

    // Called when ray hovers over button - synchronized with button highlight color change
    public void OnSelect(BaseEventData eventData)
    {
        if (selectable != null && selectable.interactable && !hasTriggeredHover)
        {
            hasTriggeredHover = true;
            TriggerHoverHaptic();
        }
    }

    // Called when button is pressed down - synchronized with button pressed color change
    public void OnPointerDown(PointerEventData eventData)
    {
        if (selectable != null && selectable.interactable)
        {
            TriggerPressHaptic();
        }
    }

    private void OnDisable()
    {
        hasTriggeredHover = false;
    }

    private void TriggerHoverHaptic()
    {
        if (HapticsManager.Instance == null) return;

        if (controller == OVRInput.Controller.RTouch)
            HapticsManager.Instance.PulseUIHoverRight();
        else if (controller == OVRInput.Controller.LTouch)
            HapticsManager.Instance.PulseUIHoverLeft();
    }

    private void TriggerPressHaptic()
    {
        if (HapticsManager.Instance == null) return;

        if (controller == OVRInput.Controller.RTouch)
            HapticsManager.Instance.PulseUIPressRight();
        else if (controller == OVRInput.Controller.LTouch)
            HapticsManager.Instance.PulseUIPressLeft();
    }
}
