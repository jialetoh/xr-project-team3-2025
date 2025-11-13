using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using nickmaltbie.OpenKCC.Character;

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
    public AnimationCurve m_Curve = new AnimationCurve();

    public delegate void LinkEvent();
    [Tooltip("Event triggered when starting to traverse an off-mesh link.")]
    public LinkEvent OnLinkStart;
    [Tooltip("Event triggered when finishing the traversal of an off-mesh link.")]
    public LinkEvent OnLinkEnd;

    [Header("Speed Settings")]

    [Tooltip("Minimum duration for traversing an off-mesh link.")]
    public float minDuration = 0.1f;
    [Tooltip("Maximum duration for traversing an off-mesh link.")]
    public float maxDuration = 5.0f;

    IEnumerator Start()
    {
        NavMeshAgent agent = GetComponent<NavMeshAgent>();

        agent.autoTraverseOffMeshLink = false;
        while (true)
        {
            if (agent.isOnOffMeshLink)
            {
                OnLinkStart?.Invoke();

                // Calculate duration based on distance and agent speed
                OffMeshLinkData data = agent.currentOffMeshLinkData;
                float distance = Vector3.Distance(data.startPos, data.endPos);
                float duration = distance / agent.speed;

                // Clamp duration to reasonable bounds
                duration = Mathf.Clamp(duration, minDuration, maxDuration);

                if (m_Method == OffMeshLinkMoveMethod.NormalSpeed)
                    yield return StartCoroutine(NormalSpeed(agent));
                else if (m_Method == OffMeshLinkMoveMethod.Parabola)
                    yield return StartCoroutine(Parabola(agent, 2.0f, duration));
                else if (m_Method == OffMeshLinkMoveMethod.Curve)
                    yield return StartCoroutine(Curve(agent, duration));

                // Complete the link and sync positions
                agent.CompleteOffMeshLink();
                OnLinkEnd?.Invoke();
            }
            yield return null;
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