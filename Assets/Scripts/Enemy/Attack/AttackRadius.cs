// Adapted from: https://github.com/llamacademy/ai-series-part-6/blob/main/Assets/Scripts/AttackRadius.cs

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SphereCollider), typeof(AudioSource))]
public class AttackRadius : MonoBehaviour
{
    [Header("Components")]
    [Tooltip("The SphereCollider component used as the attack radius.")]
    public SphereCollider Collider;
    [Tooltip("Event triggered when an attack occurs.")]
    public delegate void AttackEvent(IDamageable Target);
    [Tooltip("Event triggered when an attack occurs.")]
    public AttackEvent OnAttack;
    [Tooltip("Coroutine handling the attack process.")]
    protected Coroutine AttackCoroutine;
    [Tooltip("The list of damageable targets within the attack radius.")]
    protected List<IDamageable> Damageables = new();

    [Header("Audio")]
    [Tooltip("The AudioSource component for playing attack sounds.")]
    private AudioSource _audioSource;
    [Tooltip("The audio clip(s) played when an attack occurs.")]
    public AudioClip[] AttackAudioClips;

    [Header("Attack Settings")]
    [Tooltip("The damage dealt to each damageable target.")]
    public int Damage = 10;
    [Tooltip("The delay between consecutive attacks.")]
    public float AttackDelay = 0.5f;

    protected virtual void Awake()
    {
        Collider = GetComponent<SphereCollider>();
        _audioSource = GetComponent<AudioSource>();
    }

    protected virtual void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out IDamageable damageable))
        {
            Damageables.Add(damageable);

            if (AttackCoroutine == null)
                AttackCoroutine = StartCoroutine(Attack());
        }
    }

    protected virtual void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent(out IDamageable damageable))
        {
            Damageables.Remove(damageable);
            if (Damageables.Count == 0)
            {
                StopCoroutine(AttackCoroutine);
                AttackCoroutine = null;
            }
        }
    }

    public void Reset()
    {
        Damageables.Clear();
        if (AttackCoroutine != null)
        {
            StopCoroutine(AttackCoroutine);
            AttackCoroutine = null;
        }
    }

    protected virtual IEnumerator Attack()
    {
        WaitForSeconds Wait = new(AttackDelay);

        IDamageable closestDamageable = null;
        float closestDistance = float.MaxValue;

        while (Damageables.Count > 0)
        {
            for (int i = 0; i < Damageables.Count; i++)
            {
                Transform damageableTransform = Damageables[i].GetTransform();
                float distance = Vector3.Distance(transform.position, damageableTransform.position);

                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestDamageable = Damageables[i];
                }
            }

            if (closestDamageable != null)
            {
                OnAttack?.Invoke(closestDamageable);
                PlayAttackSound();
                closestDamageable.TakeDamage(Damage);
            }

            closestDamageable = null;
            closestDistance = float.MaxValue;

            yield return Wait;

            Damageables.RemoveAll(DisabledDamageables);
        }

        AttackCoroutine = null;
    }

    protected bool DisabledDamageables(IDamageable Damageable)
    {
        return Damageable != null && !Damageable.GetTransform().gameObject.activeSelf;
    }

    protected void PlayAttackSound()
    {
        if (AttackAudioClips.Length == 0) return;

        int index = Random.Range(0, AttackAudioClips.Length);
        _audioSource.PlayOneShot(AttackAudioClips[index]);
    }
}