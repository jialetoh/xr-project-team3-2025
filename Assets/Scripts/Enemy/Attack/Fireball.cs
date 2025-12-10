// Adapted from: https://github.com/llamacademy/ai-series-part-7/blob/main/Assets/Scripts/Bullet.cs

using UnityEngine;
using UnityEngine.VFX;

[RequireComponent(typeof(Rigidbody), typeof(AudioSource))]
public class Fireball : PoolableObject
{
    [Header("Fireball Settings")]
    [Tooltip("Time in seconds before the fireball auto-destroys itself.")]
    public float AutoDestroyTime = 5f;
    [Tooltip("Speed at which the fireball moves.")]
    public float MoveSpeed = 2f;
    [Tooltip("Damage dealt by the fireball.")]
    public int Damage = 5;
    [Tooltip("Rigidbody component of the fireball.")]
    public Rigidbody Rigidbody;

    [Header("Visual Effects")]
    [Tooltip("Visual Effects Graph asset for explosion effect.")]
    public VisualEffectAsset ExplosionVFXAsset;
    [Tooltip("Trail renderers for the fireball.")]
    private TrailRenderer[] TrailRenderers;
    [Tooltip("Duration of the explosion animation.")]
    [SerializeField]
    private float ExplosionAnimationDuration = 1.5f;

    [Header("Audio")]
    [Tooltip("The AudioSource component for playing fireball sounds.")]
    private AudioSource _audioSource;
    [Tooltip("The sound played when the fireball is in play.")]
    public AudioClip FireballSound;
    [Tooltip("The sound played upon explosion.")]
    public AudioClip ExplosionSound;

    private const string DISABLE_METHOD_NAME = "Disable";

    private void Awake()
    {
        Rigidbody = GetComponent<Rigidbody>();
        TrailRenderers = GetComponentsInChildren<TrailRenderer>();
        _audioSource = GetComponent<AudioSource>();
    }

    private void OnEnable()
    {
        CancelInvoke(DISABLE_METHOD_NAME);
        Invoke(DISABLE_METHOD_NAME, AutoDestroyTime);
        PlayFireballSound();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out IDamageable damageable))
        {
            damageable.TakeDamage(Damage);
        }

        // Play explosion effect at collision point
        PlayExplosionEffect();
        PlayExplosionSound();

        Disable();
    }
    private void PlayExplosionEffect()
    {
        if (ExplosionVFXAsset != null)
        {
            // Create a temporary GameObject for the explosion effect
            GameObject vfxObject = new("FireballExplosion");
            vfxObject.transform.SetPositionAndRotation(transform.position, transform.rotation);

            // Add and configure the VisualEffect component
            VisualEffect vfx = vfxObject.AddComponent<VisualEffect>();
            vfx.visualEffectAsset = ExplosionVFXAsset;

            // Play the effect and destroy after duration
            vfx.Play();
            Destroy(vfxObject, ExplosionAnimationDuration);
        }
    }

    private void PlayFireballSound()
    {
        if (_audioSource != null && FireballSound != null)
        {
            if (_audioSource.clip != FireballSound)
            {
                _audioSource.clip = FireballSound;
                _audioSource.loop = true;
            }
            if (!_audioSource.isPlaying)
            {
                _audioSource.Play();
            }
        }
    }

    private void PlayExplosionSound()
    {
        // _audioSource.Stop();
        // if (_audioSource != null && ExplosionSound != null)
        // {
        //     _audioSource.PlayOneShot(ExplosionSound);
        // }
        return;
    }

    private void Disable()
    {
        CancelInvoke(DISABLE_METHOD_NAME);
        Rigidbody.linearVelocity = Vector3.zero;

        foreach (TrailRenderer trailRenderer in TrailRenderers)
        {
            trailRenderer.Clear();
        }
        _audioSource.Stop();

        gameObject.SetActive(false);
    }
}