// Adapted from: https://github.com/llamacademy/ai-series-part-4/blob/main/Assets/Scripts/Enemy/EnemyMovement.cs

using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public enum MovementState
{
    Walking,
    Jumping,
    Landing,
};

[RequireComponent(typeof(NavMeshAgent), typeof(AgentLinkMover))]
public class EnemyMovement : MonoBehaviour
{
    [Tooltip("The character to follow.")]
    public Transform Player;

    [Tooltip("Animator component for handling animations.")]
    [SerializeField]
    private Animator Animator;

    [Tooltip("Rate at which to update the agent's destination.")]
    public float UpdateRate = 0.1f;

    [Tooltip("NavMeshAgent component for pathfinding.")]
    private NavMeshAgent Agent;
    [Tooltip("AgentLinkMover component for handling off-mesh link traversal.")]
    private AgentLinkMover LinkMover;

    [Tooltip("Coroutine for following the target.")]
    private Coroutine FollowCoroutine;

    private void Awake()
    {
        Agent = GetComponent<NavMeshAgent>();
        LinkMover = GetComponent<AgentLinkMover>();

        LinkMover.OnLinkEnd += HandleLinkEnd;
        LinkMover.OnLinkStart += HandleLinkStart;
    }

    public void StartChasing()
    {
        if (FollowCoroutine == null)
        {
            FollowCoroutine = StartCoroutine(FollowTarget());
        }
        else
        {
            Debug.LogWarning("Called StartChasing on Enemy that is already chasing! This is likely a bug in some calling class!");
        }
    }

    private void HandleLinkStart()
    {
        Animator.SetTrigger(MovementState.Jumping.ToString());
    }

    private void HandleLinkEnd()
    {
        Animator.SetTrigger(MovementState.Landing.ToString());
    }

    private void Update()
    {
        Animator.SetBool(MovementState.Walking.ToString(), Agent.velocity.magnitude > 0.01f);
    }

    private IEnumerator FollowTarget()
    {
        WaitForSeconds Wait = new(UpdateRate);

        while (gameObject.activeSelf)
        {
            Agent.SetDestination(Player.transform.position - (Player.transform.position - transform.position).normalized * 0.5f);
            yield return Wait;
        }
    }
}