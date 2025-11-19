// Adapted from: https://github.com/llamacademy/ai-series-part-6/blob/main/Assets/Scripts/AttackRadius.cs

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
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
    private Coroutine AttackCoroutine;
    [Tooltip("The list of damageable targets within the attack radius.")]
    private List<IDamageable> Damageables = new();

    [Header("Attack Settings")]
    [Tooltip("The damage dealt to each damageable target.")]
    public int Damage = 10;
    [Tooltip("The delay between consecutive attacks.")]
    public float AttackDelay = 0.5f;

    private void Awake()
    {
        Collider = GetComponent<SphereCollider>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<IDamageable>(out var damageable))
        {
            Damageables.Add(damageable);
            AttackCoroutine ??= StartCoroutine(Attack());
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent<IDamageable>(out var damageable))
        {
            Damageables.Remove(damageable);
            if (Damageables.Count == 0)
            {
                StopCoroutine(AttackCoroutine);
                AttackCoroutine = null;
            }
        }
    }

    private IEnumerator Attack()
    {
        WaitForSeconds Wait = new(AttackDelay);

        yield return Wait;

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
                closestDamageable.TakeDamage(Damage);
            }

            closestDamageable = null;
            closestDistance = float.MaxValue;

            yield return Wait;

            Damageables.RemoveAll(DisabledDamageables);
        }

        AttackCoroutine = null;
    }

    private bool DisabledDamageables(IDamageable Damageable)
    {
        return Damageable != null && !Damageable.GetTransform().gameObject.activeSelf;
    }
}