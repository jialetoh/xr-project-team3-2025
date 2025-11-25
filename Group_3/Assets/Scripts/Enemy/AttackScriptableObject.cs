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