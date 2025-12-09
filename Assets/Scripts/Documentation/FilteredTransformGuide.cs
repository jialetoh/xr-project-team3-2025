using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class FilteredTransformGuide : MonoBehaviour
{

}

#if UNITY_EDITOR
[CustomEditor(typeof(FilteredTransformGuide))]
public class FilteredTransformGuideEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // ---------- HEADER ----------
        GUIStyle headerStyle = new(EditorStyles.boldLabel)
        {
            fontSize = 13,
            alignment = TextAnchor.LowerLeft
        };
        EditorGUILayout.LabelField("README: Tuning the Filter Properties for the script below", headerStyle);
        // ----------------------------

        EditorGUILayout.HelpBox(
            "MIN CUTOFF (Jitter vs. Lag at Low Speed)\n" +
            "• High Value (1.0+): Less lag, but more shaky hands.\n" +
            "• Low Value (0.1)  : Very smooth stability, but 'floaty' aim.\n" +
            "👉 Tune this while holding your hand STILL.",
            MessageType.None);

        EditorGUILayout.HelpBox(
            "BETA (Responsiveness at High Speed)\n" +
            "• High Value (0.1+) : Snaps instantly during fast flicks.\n" +
            "• Low Value (0.001): Sluggish/Heavy feeling during flicks.\n" +
            "👉 Tune this while moving your hand FAST.",
            MessageType.None);

        EditorGUILayout.HelpBox(
            "D CUTOFF (Transition Smoothness)\n" +
            "• Usually keep at 1.0.\n" +
            "• Controls how smoothly the filter changes between 'Still' and 'Moving'.",
            MessageType.None);
    }
}
#endif