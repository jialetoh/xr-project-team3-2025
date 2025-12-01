using UnityEngine;
using UnityEngine.EventSystems;

public class HapticPauseMenu : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler
{
    public void OnPointerEnter(PointerEventData eventData)
    {
        // Light vibration on hover
        OVRInput.SetControllerVibration(0.2f, 0.2f, OVRInput.Controller.RTouch);
        OVRInput.SetControllerVibration(0, 0, OVRInput.Controller.RTouch);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        // Stronger vibration on click
        StartCoroutine(ClickHaptic());
    }

    private System.Collections.IEnumerator ClickHaptic()
    {
        OVRInput.SetControllerVibration(0.8f, 0.8f, OVRInput.Controller.RTouch);
        yield return new WaitForSeconds(0.1f);
        OVRInput.SetControllerVibration(0, 0, OVRInput.Controller.RTouch);
    }
}
