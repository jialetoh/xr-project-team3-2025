using UnityEngine;

public abstract class Projectile : MonoBehaviour
{
    [Header("Projectile Stats")]
    public int damage = 10;
    public float lifetime = 5f;

    [Header("Audio")]
    public AudioSource projectileAudioSource;
    public AudioClip impactClip;

    [Header("Layers")]
    public LayerMask hitLayers;

    protected float _spawnTime;

    protected virtual void Start()
    {
        _spawnTime = Time.time;
    }

    protected virtual void Update()
    {
        // Auto-destroy after lifetime
        if (Time.time - _spawnTime >= lifetime)
        {
            Destroy(gameObject);
        }
    }

    protected virtual void OnTriggerEnter(Collider other)
    {
        // Check layer
        if ((hitLayers.value & (1 << other.gameObject.layer)) == 0)
            return;

        OnHit(other);
    }

    protected virtual void OnCollisionEnter(Collision collision)
    {
        // Check layer
        if ((hitLayers.value & (1 << collision.gameObject.layer)) == 0)
            return;

        OnHit(collision.collider);
    }

    protected virtual void OnHit(Collider other)
    {
        // Deal damage
        // IDamageable damageable = other.GetComponentInParent<IDamageable>();
        // if (damageable != null) damageable.TakeDamage(damage);

        // Play impact sound
        PlayImpactSound();

        // Destroy projectile
        Destroy(gameObject);
    }

    protected virtual void PlayImpactSound()
    {
        if (projectileAudioSource != null && impactClip != null)
        {
            // Play at position since we're about to destroy
            AudioSource.PlayClipAtPoint(impactClip, transform.position);
        }
    }
}
