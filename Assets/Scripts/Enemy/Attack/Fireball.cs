// Adapted from: https://github.com/llamacademy/ai-series-part-7/blob/main/Assets/Scripts/Bullet.cs

using UnityEngine;
using UnityEngine.VFX;

[RequireComponent(typeof(Rigidbody))]
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

    private const string DISABLE_METHOD_NAME = "Disable";

    private void Awake()
    {
        Rigidbody = GetComponent<Rigidbody>();
        TrailRenderers = GetComponentsInChildren<TrailRenderer>();
    }

    private void OnEnable()
    {
        CancelInvoke(DISABLE_METHOD_NAME);
        Invoke(DISABLE_METHOD_NAME, AutoDestroyTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out IDamageable damageable))
        {
            damageable.TakeDamage(Damage);
        }

        // Play explosion effect at collision point
        PlayExplosionEffect();

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

    private void Disable()
    {
        CancelInvoke(DISABLE_METHOD_NAME);
        Rigidbody.linearVelocity = Vector3.zero;

        foreach (TrailRenderer trailRenderer in TrailRenderers)
        {
            trailRenderer.Clear();
        }

        gameObject.SetActive(false);
    }
}