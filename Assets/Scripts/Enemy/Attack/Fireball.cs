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
    [Tooltip("Pooled explosion VFX prefab (PoolableObject with VisualEffect component).")]
    public PoolableObject ExplosionVFXPrefab;
    [Tooltip("Visual Effects Graph asset for explosion effect (legacy - use ExplosionVFXPrefab instead).")]
    public VisualEffectAsset ExplosionVFXAsset;
    [Tooltip("Trail renderers for the fireball.")]
    private TrailRenderer[] TrailRenderers;
    [Tooltip("Duration of the explosion animation.")]
    [SerializeField]
    private float ExplosionAnimationDuration = 1.5f;

    private static ObjectPool _explosionPool;

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

        // Delay disable to allow explosion sound to play
        // Use ExplosionAnimationDuration or a minimum delay for the sound
        float disableDelay = Mathf.Max(ExplosionAnimationDuration, 0.5f);
        Invoke(DISABLE_METHOD_NAME, disableDelay);
    }
    private void PlayExplosionEffect()
    {
        // Use pooled explosion prefab if available (optimized)
        if (ExplosionVFXPrefab != null)
        {
            // Initialize pool on first use
            if (_explosionPool == null)
            {
                _explosionPool = ObjectPool.CreateInstance(ExplosionVFXPrefab, 10);
            }

            // Get pooled explosion VFX
            PoolableObject explosionVFX = _explosionPool.GetObject();
            explosionVFX.transform.SetPositionAndRotation(transform.position, transform.rotation);

            // Play the VFX if it has a VisualEffect component
            if (explosionVFX.TryGetComponent<VisualEffect>(out var vfx))
            {
                vfx.Play();
            }

            // Return to pool after animation completes
            if (explosionVFX.gameObject.activeSelf)
            {
                StartCoroutine(DisableAfterDelay(explosionVFX.gameObject, ExplosionAnimationDuration));
            }
        }
        // Legacy fallback: Use VisualEffectAsset (creates new GameObject - not optimized)
        else if (ExplosionVFXAsset != null)
        {
            GameObject vfxObject = new("FireballExplosion");
            vfxObject.transform.SetPositionAndRotation(transform.position, transform.rotation);

            VisualEffect vfx = vfxObject.AddComponent<VisualEffect>();
            vfx.visualEffectAsset = ExplosionVFXAsset;

            vfx.Play();
            Destroy(vfxObject, ExplosionAnimationDuration);
        }
    }

    private System.Collections.IEnumerator DisableAfterDelay(GameObject obj, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (obj != null)
        {
            obj.SetActive(false);
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
        if (_audioSource != null && ExplosionSound != null)
        {
            _audioSource.PlayOneShot(ExplosionSound);
        }
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