using UnityEngine;

// public interface IDamageable
// {
//     // Please put this inside enemies, so that the weapons can deal damage
//     void TakeDamage(int amount);
// }
public class SwordWeapon : Weapon
{
    // Parameters
    public int damage = 25;
    public float minHitVelocity = 1.0f;
    public LayerMask hittableLayers;

    [Header("Audio")]
    public AudioSource swordAudioSource;
    public AudioClip swordHitSound;

    private RightControllerInteractor _rightInteractor;
    private Vector3 _lastPosition;
    private Vector3 _velocity;


    void Awake()
    {
        _rightInteractor = GetComponentInParent<RightControllerInteractor>();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {

    }

    // Update is called once per frame
    private void Update()
    {

    }

    void OnTriggerEnter(Collider other)
    {
        // Only count hits if we're actually swinging
        if (_velocity.magnitude < minHitVelocity)
            return;

        // Layer filter
        if ((hittableLayers.value & (1 << other.gameObject.layer)) == 0)
            return;

        // Damage the target
        //IDamageable damageable = other.GetComponentInParent<IDamageable>();
        //damageable.TakeDamage(damage);

        // Play hit sound
        swordAudioSource.PlayOneShot(swordHitSound);

        // Haptics on right controller
        _rightInteractor.PulseHaptics(0.5f, 0.08f);
    }

    public override void OnEquip()
    {
        Debug.Log("Equipping sword");
        base.OnEquip();
        _lastPosition = transform.position;
    }
    // public virtual void OnUnequip() { }
    // public virtual void OnPrimaryFireDown() { }
    // public virtual void OnPrimaryFireUp() { }
    // public virtual void OnReload() { }
    // public virtual void OnAltAction() { }
}
