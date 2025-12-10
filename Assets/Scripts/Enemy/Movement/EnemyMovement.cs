// Adapted from: https://github.com/llamacademy/ai-series-part-4/blob/main/Assets/Scripts/Enemy/EnemyMovement.cs

using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent), typeof(AgentLinkMover))]
public class EnemyMovement : MonoBehaviour
{
    [Header("References")]
    [Tooltip("The character to follow.")]
    public Transform Player;
    [Tooltip("Line of sight checker for the enemy.")]
    public EnemyLineOfSightChecker LineOfSightChecker;

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

    [Header("Enemy State Settings")]
    [Tooltip("The default state of the enemy.")]
    public EnemyState DefaultState;
    [Tooltip("Current state of the enemy.")]
    private EnemyState _state;
    [Tooltip("Gets or sets the current state of the enemy.")]
    public EnemyState State
    {
        get
        {
            return _state;
        }
        set
        {
            OnStateChange?.Invoke(_state, value);
            _state = value;
        }
    }

    [Tooltip("Event triggered when the enemy's state changes.")]
    public delegate void StateChangeEvent(EnemyState oldState, EnemyState newState);
    [Tooltip("Event invoked on state change.")]
    public StateChangeEvent OnStateChange;

    [Header("Idle and Patrol Settings")]
    [Tooltip("Radius for idle movement.")]
    public float IdleLocationRadius = 4f;
    [Tooltip("Movement speed multiplier when idle.")]
    public float IdleMovespeedMultiplier = 0.5f;
    [Tooltip("Waypoints for Patrolling (if needed)")]
    public Vector3[] Waypoints = new Vector3[4];
    [Tooltip("Current waypoint index")]
    [SerializeField]
    private int WaypointIndex = 0;

    [Header("State Transition Settings")]
    [Tooltip("Time interval for checking state transitions (in seconds).")]
    public float StateTransitionInterval = 5f;
    [Tooltip("Chance to switch from Patrol to Idle (0-1).")]
    [Range(0f, 1f)]
    public float PatrolToIdleChance = 0.25f;
    [Tooltip("Chance to switch from Idle to Patrol (0-1).")]
    [Range(0f, 1f)]
    public float IdleToPatrolChance = 0.45f;

    [Header("Audio")]
    [Tooltip("AudioSource component for playing groan sounds.")]
    public AudioSource GroanAudioSource;
    [Tooltip("Audio clip(s) played throughout enemy lifecycle.")]
    public AudioClip[] GroanAudioClips;

    [Tooltip("Coroutine for following the target.")]
    private Coroutine FollowCoroutine;
    [Tooltip("Coroutine for handling state transitions.")]
    private Coroutine StateTransitionCoroutine;
    [Tooltip("Coroutine for playing random groan sounds.")]
    private Coroutine GroanSoundCoroutine;

    private void Awake()
    {
        Agent = GetComponent<NavMeshAgent>();
        LinkMover = GetComponent<AgentLinkMover>();

        LinkMover.OnLinkEnd += HandleLinkEnd;
        LinkMover.OnLinkStart += HandleLinkStart;

        LineOfSightChecker.OnGainSight += HandleGainSight;
        LineOfSightChecker.OnLoseSight += HandleLoseSight;

        OnStateChange += HandleStateChange;
    }

    private void OnDisable()
    {
        _state = DefaultState; // use _state to avoid triggering OnStateChange when recycling object in the pool

        StopStateTransitionCoroutine();
        StopGroanCoroutine();
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

        OnStateChange?.Invoke(EnemyState.Spawn, DefaultState);
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
            Animator.SetBool(GetStateMoveAnimation(), true);
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
            Animator.SetBool(GetStateMoveAnimation(), Agent.velocity.magnitude > 0.01f);
        }
    }

    private string GetStateMoveAnimation()
    {
        // Only set Running animation when in Chase state
        // For the rest, use Walking animation
        return State == EnemyState.Chase ? MovementState.Running.ToString() : MovementState.Walking.ToString();
    }

    /// <summary>
    /// Handles changes in the enemy's state.
    /// </summary>
    /// <param name="oldState"></param>
    /// <param name="newState"></param>
    private void HandleStateChange(EnemyState oldState, EnemyState newState)
    {
        if (oldState == newState)
        {
            return;
        }

        // Stop any existing movement coroutine
        if (FollowCoroutine != null)
        {
            StopCoroutine(FollowCoroutine);
        }
        if (oldState == EnemyState.Idle || oldState == EnemyState.Patrol)
        {
            Agent.speed /= IdleMovespeedMultiplier;
        }

        // Reset animator - turn off previous state animations
        if (oldState == EnemyState.Chase)
        {
            // Turning off running animation when leaving chase state
            Animator.SetBool(MovementState.Running.ToString(), false);
        }
        else if (oldState == EnemyState.Idle || oldState == EnemyState.Patrol)
        {
            // Turning off walking animation when leaving idle/patrol state
            Animator.SetBool(MovementState.Walking.ToString(), false);
        }

        // Start the appropriate coroutines based on the new state
        switch (newState)
        {
            case EnemyState.Idle:
                Agent.speed *= IdleMovespeedMultiplier;
                FollowCoroutine = StartCoroutine(DoIdleMotion());
                StartStateTransitionCoroutine();
                StartGroanCoroutine();
                break;
            case EnemyState.Patrol:
                Agent.speed *= IdleMovespeedMultiplier;
                FollowCoroutine = StartCoroutine(DoPatrolMotion());
                StartStateTransitionCoroutine();
                StartGroanCoroutine();
                break;
            case EnemyState.Chase:
                FollowCoroutine = StartCoroutine(FollowTarget());
                StopStateTransitionCoroutine();
                StopGroanCoroutine();
                break;
        }
    }

    private IEnumerator DoIdleMotion()
    {
        WaitForSeconds Wait = new(UpdateRate);

        while (true)
        {
            if (!Agent.enabled || !Agent.isOnNavMesh)
            {
                yield return Wait;
            }
            else if (Agent.remainingDistance <= Agent.stoppingDistance)
            {
                Vector2 point = Random.insideUnitCircle * IdleLocationRadius;
                if (NavMesh.SamplePosition(Agent.transform.position + new Vector3(point.x, 0, point.y), out NavMeshHit hit, 2f, Agent.areaMask))
                {
                    Agent.SetDestination(hit.position);
                }
            }

            yield return Wait;
        }
    }

    private IEnumerator DoPatrolMotion()
    {
        WaitForSeconds Wait = new(UpdateRate);

        yield return new WaitUntil(() => Agent.enabled && Agent.isOnNavMesh);
        Agent.SetDestination(Waypoints[WaypointIndex]);

        while (true)
        {
            if (Agent.isOnNavMesh && Agent.enabled && Agent.remainingDistance <= Agent.stoppingDistance)
            {
                WaypointIndex++;
                if (WaypointIndex >= Waypoints.Length)
                {
                    WaypointIndex = 0;
                }

                Agent.SetDestination(Waypoints[WaypointIndex]);
            }

            yield return Wait;
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

    /// <summary>
    /// Starts the coroutine to handle state transitions between Idle and Patrol.
    /// </summary>
    private void StartStateTransitionCoroutine()
    {
        // Stop existing coroutine if running
        StopStateTransitionCoroutine();

        // Start new state transition coroutine
        StateTransitionCoroutine = StartCoroutine(HandleStateTransitions());
    }

    /// <summary>
    /// Stops the state transition coroutine if it is running.
    /// </summary>
    private void StopStateTransitionCoroutine()
    {
        if (StateTransitionCoroutine == null) return;

        StopCoroutine(StateTransitionCoroutine);
        StateTransitionCoroutine = null;
    }

    /// <summary>
    /// Handles random state transitions between Idle and Patrol states.
    /// </summary>
    private IEnumerator HandleStateTransitions()
    {
        WaitForSeconds wait = new(StateTransitionInterval);

        while (true)
        {
            yield return wait;

            // Only transition between Idle and Patrol states (not during Chase)
            if (State == EnemyState.Idle || State == EnemyState.Patrol)
            {
                float randomValue = Random.Range(0f, 1f);

                if (State == EnemyState.Idle && randomValue < IdleToPatrolChance)
                {
                    // Switch from Idle to Patrol
                    State = EnemyState.Patrol;
                }
                else if (State == EnemyState.Patrol && randomValue < PatrolToIdleChance)
                {
                    // Switch from Patrol to Idle
                    State = EnemyState.Idle;
                }
            }
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

    private void HandleGainSight(Player player)
    {
        State = EnemyState.Chase;
    }

    private void HandleLoseSight(Player player)
    {
        State = DefaultState;
    }

    /// <summary>
    /// Starts the coroutine to play random groan sounds while idle or patrolling.
    /// </summary>
    private void StartGroanCoroutine()
    {
        // Stop existing coroutine if running
        StopGroanCoroutine();

        // Start new groan coroutine
        GroanSoundCoroutine = StartCoroutine(PlayRandomGroanSounds());
    }

    /// <summary>
    /// Stops the groan sound coroutine if it is running.
    /// </summary>
    private void StopGroanCoroutine()
    {
        if (GroanSoundCoroutine == null) return;

        StopCoroutine(GroanSoundCoroutine);
        GroanSoundCoroutine = null;
    }

    /// <summary>
    /// Plays random groan sounds while idle or patrolling.
    /// </summary>
    private IEnumerator PlayRandomGroanSounds()
    {
        while (true)
        {
            // Wait for a random delay between 5-10 seconds
            float randomDelay = Random.Range(5f, 10f);
            yield return new WaitForSeconds(randomDelay);

            // Play a groan sound
            PlayGroanSound();
        }
    }

    /// <summary>
    /// Plays a random groan sound.
    /// </summary>
    private void PlayGroanSound()
    {
        if (GroanAudioClips.Length == 0) return;

        AudioClip clip = GroanAudioClips[Random.Range(0, GroanAudioClips.Length)];
        GroanAudioSource.clip = clip;
        GroanAudioSource.PlayOneShot(GroanAudioSource.clip);
    }
}