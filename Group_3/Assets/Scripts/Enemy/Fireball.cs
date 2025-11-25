// Adapted from: https://github.com/llamacademy/ai-series-part-7/blob/main/Assets/Scripts/Bullet.cs

using System;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Fireball : PoolableObject
{
    [Tooltip("Time in seconds before the fireball auto-destroys itself.")]
    public float AutoDestroyTime = 5f;
    [Tooltip("Speed at which the fireball moves.")]
    public float MoveSpeed = 2f;
    [Tooltip("Damage dealt by the fireball.")]
    public int Damage = 5;
    [Tooltip("Rigidbody component of the fireball.")]
    public Rigidbody Rigidbody;

    private TrailRenderer[] TrailRenderers;

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

        Disable();
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