// Adapted from: https://github.com/llamacademy/ai-series-part-19/blob/master/Assets/Scripts/ScalingScriptableObject.cs

using UnityEngine;

[CreateAssetMenu(fileName = "Scaling Configuration", menuName = "ScriptableObject/Scaling Configuration")]
public class ScalingScriptableObject : ScriptableObject
{
    [Tooltip("Curve defining how enemy attributes scale with difficulty level.")]
    public AnimationCurve HealthCurve;
    [Tooltip("Curve defining how enemy attributes scale with difficulty level.")]
    public AnimationCurve DamageCurve;
    [Tooltip("Curve defining how enemy attributes scale with difficulty level.")]
    public AnimationCurve SpeedCurve;
    [Tooltip("Curve defining how spawn rates scale with difficulty level.")]
    public AnimationCurve SpawnRateCurve;
    [Tooltip("Curve defining how the number of enemies spawned scales with difficulty level.")]
    public AnimationCurve SpawnCountCurve;
}