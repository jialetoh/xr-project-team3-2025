// Adapted from: https://github.com/llamacademy/ai-series-part-11/blob/master/Assets/Scripts/Enemy/EnemyState.cs

/// <summary>
/// Defines the various states an enemy can be in.
/// </summary>
public enum EnemyState
{
    /// <summary>
    /// The enemy is spawning into the game world.
    /// </summary>
    Spawn,

    /// <summary>
    /// The enemy is idle and not actively pursuing the player.
    /// </summary>
    Idle,

    /// <summary>
    /// The enemy is patrolling an area.
    /// </summary>
    Patrol,

    /// <summary>
    /// The enemy is chasing the player.
    /// </summary>
    Chase
}