/// <summary>
/// Defines the various movement animation states an enemy can be in.
/// These states are synced with the enemy's animator to control movement animations.
/// </summary>
public enum MovementState
{
    /// <summary>
    /// The enemy is walking.
    /// </summary>
    Walking,

    /// <summary>
    /// The enemy is running.
    /// </summary>
    Running,

    /// <summary>
    /// The enemy is jumping.
    /// </summary>
    Jumping,

    /// <summary>
    /// The enemy is landing.
    /// </summary>
    Landing,
};