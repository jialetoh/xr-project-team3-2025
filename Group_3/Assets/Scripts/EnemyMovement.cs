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

    private void Awake()
    {
        Agent = GetComponent<NavMeshAgent>();
        LinkMover = GetComponent<AgentLinkMover>();

        LinkMover.OnLinkEnd += HandleLinkEnd;
        LinkMover.OnLinkStart += HandleLinkStart;
    }

    private void Start()
    {
        StartCoroutine(FollowTarget());
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
        WaitForSeconds Wait = new WaitForSeconds(UpdateRate);

        while (enabled)
        {
            Agent.SetDestination(Player.transform.position - (Player.transform.position - transform.position).normalized * 0.5f);
            yield return Wait;
        }
    }
}