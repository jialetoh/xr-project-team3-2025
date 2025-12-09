// Adapted from: https://github.com/llamacademy/ai-series-part-19/blob/master/Assets/Scripts/AgentLinkMover.cs

using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public enum OffMeshLinkMoveMethod
{
    Teleport,
    NormalSpeed,
    Parabola,
    Curve
}

[RequireComponent(typeof(NavMeshAgent))]
public class AgentLinkMover : MonoBehaviour
{
    [Header("OffMesh Link Movement Settings")]
    public OffMeshLinkMoveMethod m_Method = OffMeshLinkMoveMethod.Parabola;

    [Tooltip("Animation curve to use when the Curve method is selected.")]
    public AnimationCurve m_Curve = new();

    public delegate void LinkEvent(OffMeshLinkMoveMethod MoveMethod);
    [Tooltip("Event triggered when starting to traverse an off-mesh link.")]
    public LinkEvent OnLinkStart;
    [Tooltip("Event triggered when finishing the traversal of an off-mesh link.")]
    public LinkEvent OnLinkEnd;

    [Header("Speed Settings")]

    [Tooltip("Minimum duration for traversing an off-mesh link.")]
    public float minDuration = 0.1f;
    [Tooltip("Maximum duration for traversing an off-mesh link.")]
    public float maxDuration = 5.0f;

    private NavMeshAgent agent;
    private Coroutine linkMoverCoroutine;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    void OnEnable()
    {
        // Start the link monitoring coroutine
        if (linkMoverCoroutine != null)
        {
            StopCoroutine(linkMoverCoroutine);
        }
        linkMoverCoroutine = StartCoroutine(MonitorOffMeshLinks());
    }

    void OnDisable()
    {
        // Stop the coroutine when object is disabled
        if (linkMoverCoroutine != null)
        {
            StopCoroutine(linkMoverCoroutine);
            linkMoverCoroutine = null;
        }
    }

    IEnumerator MonitorOffMeshLinks()
    {
        while (true)
        {
            if (agent == null || !agent.isOnOffMeshLink)
            {
                yield return null;
                continue;
            }

            // Calculate duration based on distance and agent speed
            OffMeshLinkData data = agent.currentOffMeshLinkData;
            float distance = Vector3.Distance(data.startPos, data.endPos);
            float duration = distance / agent.speed;

            // Clamp duration to reasonable bounds
            duration = Mathf.Clamp(duration, minDuration, maxDuration);

            OnLinkStart?.Invoke(m_Method);

            if (m_Method == OffMeshLinkMoveMethod.NormalSpeed)
                yield return StartCoroutine(NormalSpeed(agent));
            else if (m_Method == OffMeshLinkMoveMethod.Parabola)
                yield return StartCoroutine(Parabola(agent, 2.0f, duration));
            else if (m_Method == OffMeshLinkMoveMethod.Curve)
                yield return StartCoroutine(Curve(agent, duration));

            OnLinkEnd?.Invoke(m_Method);

            agent.CompleteOffMeshLink();
        }
    }

    IEnumerator NormalSpeed(NavMeshAgent agent)
    {
        OffMeshLinkData data = agent.currentOffMeshLinkData;
        Vector3 endPos = data.endPos + Vector3.up * agent.baseOffset;
        while (agent.transform.position != endPos)
        {
            agent.transform.position = Vector3.MoveTowards(agent.transform.position, endPos, agent.speed * Time.deltaTime);
            yield return null;
        }
    }

    IEnumerator Parabola(NavMeshAgent agent, float height, float duration)
    {
        OffMeshLinkData data = agent.currentOffMeshLinkData;
        Vector3 startPos = agent.transform.position;
        Vector3 endPos = data.endPos + Vector3.up * agent.baseOffset;
        float normalizedTime = 0.0f;
        while (normalizedTime < 1.0f)
        {
            float yOffset = height * 4.0f * (normalizedTime - normalizedTime * normalizedTime);
            agent.transform.position = Vector3.Lerp(startPos, endPos, normalizedTime) + yOffset * Vector3.up;
            normalizedTime += Time.deltaTime / duration;
            yield return null;
        }
    }

    IEnumerator Curve(NavMeshAgent agent, float duration)
    {
        OffMeshLinkData data = agent.currentOffMeshLinkData;
        Vector3 startPos = agent.transform.position;
        Vector3 endPos = data.endPos + Vector3.up * agent.baseOffset;
        float normalizedTime = 0.0f;
        while (normalizedTime < 1.0f)
        {
            float yOffset = m_Curve.Evaluate(normalizedTime);
            agent.transform.position = Vector3.Lerp(startPos, endPos, normalizedTime) + yOffset * Vector3.up;
            normalizedTime += Time.deltaTime / duration;
            yield return null;
        }
    }
}