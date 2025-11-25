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

    [Header("Pathfinding Components")]
    [Tooltip("NavMeshAgent component for pathfinding.")]
    private NavMeshAgent Agent;
    [Tooltip("AgentLinkMover component for handling off-mesh link traversal.")]
    private AgentLinkMover LinkMover;

    [Tooltip("Triangulation data of the NavMesh for random waypoint generation.")]
    public NavMeshTriangulation Triangulation = new();

    [Tooltip("Waypoints for Patrolling (if needed)")]
    public Vector3[] Waypoints = new Vector3[4];
    [Tooltip("Current waypoint index")]
    [SerializeField]
    private int WaypointIndex = 0;

    [Tooltip("Coroutine for following the target.")]
    private Coroutine FollowCoroutine;

    private void Awake()
    {
        Agent = GetComponent<NavMeshAgent>();
        LinkMover = GetComponent<AgentLinkMover>();

        LinkMover.OnLinkEnd += HandleLinkEnd;
        LinkMover.OnLinkStart += HandleLinkStart;
    }

    public void Spawn()
    {
        if (Triangulation.vertices != null)
        {
            for (int i = 0; i < Waypoints.Length; i++)
            {
                if (NavMesh.SamplePosition(Triangulation.vertices[Random.Range(0, Triangulation.vertices.Length)], out NavMeshHit Hit, 2f, Agent.areaMask))
                {
                    Waypoints[i] = Hit.position;
                }
                else
                {
                    Debug.LogError("Unable to find position for navmesh near Triangulation vertex!");
                }
            }
        }
    }

    public void StartChasing()
    {
        if (FollowCoroutine != null)
        {
            StopCoroutine(FollowCoroutine);
        }

        FollowCoroutine = StartCoroutine(FollowTarget());
    }

    private void HandleLinkStart(OffMeshLinkMoveMethod MoveMethod)
    {
        if (MoveMethod == OffMeshLinkMoveMethod.NormalSpeed)
        {
            Animator.SetBool(MovementState.Walking.ToString(), true);
        }
        else if (MoveMethod != OffMeshLinkMoveMethod.Teleport)
        {
            Animator.SetTrigger(MovementState.Jumping.ToString());
        }
    }

    private void HandleLinkEnd(OffMeshLinkMoveMethod MoveMethod)
    {
        if (MoveMethod != OffMeshLinkMoveMethod.Teleport && MoveMethod != OffMeshLinkMoveMethod.NormalSpeed)
        {
            Animator.SetTrigger(MovementState.Landing.ToString());
        }
    }

    private void Update()
    {
        if (!Agent.isOnOffMeshLink)
        {
            Animator.SetBool(MovementState.Walking.ToString(), Agent.velocity.magnitude > 0.01f);
        }
    }

    private IEnumerator FollowTarget()
    {
        WaitForSeconds Wait = new(UpdateRate);

        while (true)
        {
            if (Agent.enabled)
            {
                Agent.SetDestination(Player.transform.position - (Player.transform.position - transform.position).normalized * 0.5f);
            }

            yield return Wait;
        }
    }

    private void OnDrawGizmosSelected()
    {
        for (int i = 0; i < Waypoints.Length; i++)
        {
            Gizmos.DrawWireSphere(Waypoints[i], 0.25f);
            if (i + 1 < Waypoints.Length)
            {
                Gizmos.DrawLine(Waypoints[i], Waypoints[i + 1]);
            }
            else
            {
                Gizmos.DrawLine(Waypoints[i], Waypoints[0]);
            }
        }
    }
}