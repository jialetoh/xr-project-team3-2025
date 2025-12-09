// Adapted from: https://github.com/llamacademy/ai-series-part-20/blob/main/Assets/Scripts/WeightedSpawnScriptableObject.cs

using UnityEngine;

[CreateAssetMenu(fileName = "Weighted Spawn Config", menuName = "ScriptableObject/Weighted Spawn Config")]
public class WeightedSpawnScriptableObject : ScriptableObject
{
    [Tooltip("The enemy type to spawn.")]
    public EnemyScriptableObject Enemy;
    [Tooltip("Minimum weight for spawning this enemy type.")]
    [Range(0, 1)]
    public float MinWeight;
    [Tooltip("Maximum weight for spawning this enemy type.")]
    [Range(0, 1)]
    public float MaxWeight;

    public float GetWeight()
    {
        return Random.Range(MinWeight, MaxWeight);
    }
}