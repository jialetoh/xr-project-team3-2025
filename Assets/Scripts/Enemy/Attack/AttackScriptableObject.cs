// Adapted from: https://github.com/llamacademy/ai-series-part-18/blob/master/Assets/Scripts/Enemy/AttackScriptableObject.cs

using UnityEngine;

[CreateAssetMenu(fileName = "Attack Configuration", menuName = "ScriptableObject/Attack Configuration")]
public class AttackScriptableObject : ScriptableObject
{
    [Header("General Attack Settings")]
    [Tooltip("Is the attack a ranged attack?")]
    public bool IsRanged = false;
    [Tooltip("The damage dealt by the attack")]
    public int Damage = 5;
    [Tooltip("The radius of the attack")]
    public float AttackRadius = 1.5f;
    [Tooltip("The delay between attacks")]
    public float AttackDelay = 1.5f;

    // Ranged Configs
    [Header("Ranged Attack Settings")]
    [Tooltip("The fireball prefab to be used for ranged attacks.")]
    public Fireball FireballPrefab;
    [Tooltip("The height offset for spawning the fireball.")]
    public float SpawnHeightOffset = 1f;
    [Tooltip("The layer mask used for line of sight checks.")]
    public LayerMask LineOfSightLayers;

    /// <summary>
    /// Scales up the attack configuration based on the provided scaling configuration and level.
    /// </summary>
    /// <param name="Scaling">Scaling configuration</param>
    /// <param name="Level">Level to scale up for</param>
    /// <returns>Scaled up attack configuration</returns>
    public AttackScriptableObject ScaleUpForLevel(ScalingScriptableObject Scaling, int Level)
    {
        AttackScriptableObject scaledUpConfiguration = CreateInstance<AttackScriptableObject>();

        scaledUpConfiguration.IsRanged = IsRanged;
        scaledUpConfiguration.Damage = Mathf.FloorToInt(Damage * Scaling.DamageCurve.Evaluate(Level));
        scaledUpConfiguration.AttackRadius = AttackRadius;
        scaledUpConfiguration.AttackDelay = AttackDelay;

        scaledUpConfiguration.FireballPrefab = FireballPrefab;
        scaledUpConfiguration.SpawnHeightOffset = SpawnHeightOffset;
        scaledUpConfiguration.LineOfSightLayers = LineOfSightLayers;

        return scaledUpConfiguration;
    }

    public void SetupEnemy(Enemy enemy)
    {
        (enemy.AttackRadius.Collider == null ? enemy.AttackRadius.GetComponent<SphereCollider>() : enemy.AttackRadius.Collider).radius = AttackRadius;
        enemy.AttackRadius.AttackDelay = AttackDelay;
        enemy.AttackRadius.Damage = Damage;

        if (IsRanged)
        {
            RangedAttackRadius rangedAttackRadius = enemy.AttackRadius.GetComponent<RangedAttackRadius>();

            rangedAttackRadius.FireballPrefab = FireballPrefab;
            rangedAttackRadius.SpawnHeightOffset = SpawnHeightOffset;
            rangedAttackRadius.Mask = LineOfSightLayers;

            rangedAttackRadius.CreateFireballPool();
        }
    }
}