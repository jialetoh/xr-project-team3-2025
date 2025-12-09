// Adapted from: https://github.com/llamacademy/ai-series-part-11/blob/master/Assets/Scripts/Enemy/EnemyLineOfSightChecker.cs

using System.Collections;
using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class EnemyLineOfSightChecker : MonoBehaviour
{
    [Header("Line of Sight Settings")]
    [Tooltip("SphereCollider used for line of sight detection.")]
    public SphereCollider Collider;
    [Tooltip("Field of view angle in degrees.")]
    public float FieldOfView = 90f;
    [Tooltip("Layers considered for line of sight checks.")]
    public LayerMask LineOfSightLayers;

    [Header("Line of Sight Events")]
    [Tooltip("Event invoked on gaining sight of the player.")]
    public GainSightEvent OnGainSight;
    [Tooltip("Event triggered when the enemy gains sight of the player.")]
    public delegate void GainSightEvent(Player player);
    [Tooltip("Event invoked on losing sight of the player.")]
    public delegate void LoseSightEvent(Player player);
    [Tooltip("Event triggered when the enemy loses sight of the player.")]
    public LoseSightEvent OnLoseSight;

    [Tooltip("Coroutine for checking line of sight.")]
    private Coroutine CheckForLineOfSightCoroutine;

    private void Awake()
    {
        Collider = GetComponent<SphereCollider>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out Player player))
        {
            if (!CheckLineOfSight(player))
            {
                CheckForLineOfSightCoroutine = StartCoroutine(CheckForLineOfSight(player));
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent(out Player player))
        {
            OnLoseSight?.Invoke(player);
            if (CheckForLineOfSightCoroutine != null)
            {
                StopCoroutine(CheckForLineOfSightCoroutine);
            }
        }
    }

    private IEnumerator CheckForLineOfSight(Player player)
    {
        WaitForSeconds Wait = new(0.1f);

        while (!CheckLineOfSight(player))
        {
            yield return Wait;
        }
    }

    private bool CheckLineOfSight(Player player)
    {
        Vector3 Direction = (player.transform.position - transform.position).normalized;
        float DotProduct = Vector3.Dot(transform.forward, Direction);
        if (DotProduct >= Mathf.Cos(FieldOfView))
        {

            if (Physics.Raycast(transform.position, Direction, out RaycastHit Hit, Collider.radius, LineOfSightLayers))
            {
                if (Hit.transform.GetComponent<Player>() != null)
                {
                    OnGainSight?.Invoke(player);
                    return true;
                }
            }
        }

        return false;
    }
}
