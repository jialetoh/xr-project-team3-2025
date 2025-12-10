using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class EnemyMovementAudioHandler : MonoBehaviour
{
    [Tooltip("Audio source for movement sounds.")]
    private AudioSource _movementAudioSource;
    [Tooltip("Audio clip(s) played when the enemy walks.")]
    public AudioClip[] WalkAudioClips;
    [Tooltip("Audio clip(s) played when the enemy runs.")]
    public AudioClip[] RunAudioClips;
    [Tooltip("Audio clip(s) played when the enemy jumps.")]
    public AudioClip[] JumpAudioClips;
    [Tooltip("Audio clip(s) played when the enemy attacks.")]
    public AudioClip[] AttackAudioClips;
    [Tooltip("The audio clip played when the enemy dies.")]
    public AudioClip DeathAudioClip;

    private void Awake()
    {
        _movementAudioSource = GetComponent<AudioSource>();
    }

    /// <summary>
    /// This is an animation event called when the enemy walks.
    /// </summary>
    public void PlayWalkSound()
    {
        PlayRandomSound(WalkAudioClips);
    }

    /// <summary>
    /// This is an animation event called when the enemy runs.
    /// </summary>
    public void PlayRunSound()
    {
        PlayRandomSound(RunAudioClips);
    }

    /// <summary>
    /// This is an animation event called when the enemy jumps.
    /// </summary>
    public void PlayJumpSound()
    {
        PlayRandomSound(JumpAudioClips);
    }

    /// <summary>
    /// This is an animation event called when the enemy attacks.
    /// </summary>
    public void PlayAttackSound()
    {
        PlayRandomSound(AttackAudioClips);
    }

    /// <summary>
    /// This is an animation event called when the enemy dies.
    /// </summary>
    public void PlayDeathSound()
    {
        if (DeathAudioClip == null) return;

        _movementAudioSource.PlayOneShot(DeathAudioClip);
    }

    /// <summary>
    /// Plays a random sound from the provided array of audio clips.
    /// </summary>
    /// <param name="clips">The list of audio clips to select from.</param>
    private void PlayRandomSound(AudioClip[] clips)
    {
        if (clips.Length == 0) return;

        AudioClip clip = clips[Random.Range(0, clips.Length)];
        _movementAudioSource.clip = clip;
        _movementAudioSource.PlayOneShot(_movementAudioSource.clip);
    }
}