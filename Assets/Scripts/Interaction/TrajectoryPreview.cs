using UnityEngine;

public class TrajectoryPreview : MonoBehaviour
{
    [Header("Trajectory Line")]
    public LineRenderer trajectoryLine;
    public int lineSegments = 30;
    public float timeStep = 0.1f;

    [Header("Impact Indicator")]
    public GameObject impactIndicator;        // Sphere/circle showing landing point
    public GameObject aoeRadiusIndicator;     // Circle showing explosion radius

    [Header("Visual Settings")]
    public Color trajectoryColor = new Color(1f, 0.5f, 0f, 0.5f);
    public Color aoeColor = new Color(1f, 0f, 0f, 0.3f);

    private Vector3[] _trajectoryPoints;
    private bool _isShowing = false;

    private void Awake()
    {
        _trajectoryPoints = new Vector3[lineSegments];

        // Setup line renderer if not assigned
        if (trajectoryLine == null)
        {
            trajectoryLine = gameObject.AddComponent<LineRenderer>();
            trajectoryLine.startWidth = 0.02f;
            trajectoryLine.endWidth = 0.02f;
            trajectoryLine.material = new Material(Shader.Find("Sprites/Default"));
            trajectoryLine.startColor = trajectoryColor;
            trajectoryLine.endColor = trajectoryColor;
        }

        trajectoryLine.positionCount = lineSegments;
        Hide();
    }

    public void Show()
    {
        _isShowing = true;
        trajectoryLine.enabled = true;
        if (impactIndicator != null) impactIndicator.SetActive(true);
        if (aoeRadiusIndicator != null) aoeRadiusIndicator.SetActive(true);
    }

    public void Hide()
    {
        _isShowing = false;
        trajectoryLine.enabled = false;
        if (impactIndicator != null) impactIndicator.SetActive(false);
        if (aoeRadiusIndicator != null) aoeRadiusIndicator.SetActive(false);
    }

    /// <summary>
    /// Updates the trajectory preview for an arc projectile
    /// </summary>
    /// <param name="startPosition">Where the projectile will spawn</param>
    /// <param name="initialVelocity">Initial velocity vector</param>
    /// <param name="explosionRadius">Radius for AOE indicator (0 to hide)</param>
    /// <param name="groundLayer">Layer mask for ground detection</param>
    public void UpdateTrajectory(Vector3 startPosition, Vector3 initialVelocity, float explosionRadius, LayerMask groundLayer)
    {
        if (!_isShowing) return;

        Vector3 currentPos = startPosition;
        Vector3 currentVel = initialVelocity;
        Vector3 gravity = Physics.gravity;

        Vector3 impactPoint = startPosition;
        bool foundImpact = false;

        for (int i = 0; i < lineSegments; i++)
        {
            _trajectoryPoints[i] = currentPos;

            // Check for ground collision
            Vector3 nextPos = currentPos + currentVel * timeStep + 0.5f * gravity * timeStep * timeStep;

            if (!foundImpact)
            {
                RaycastHit hit;
                if (Physics.Raycast(currentPos, (nextPos - currentPos).normalized, out hit, (nextPos - currentPos).magnitude, groundLayer))
                {
                    impactPoint = hit.point;
                    foundImpact = true;

                    // Fill remaining points with impact point
                    for (int j = i; j < lineSegments; j++)
                    {
                        _trajectoryPoints[j] = impactPoint;
                    }
                    break;
                }
            }

            // Update for next iteration
            currentVel += gravity * timeStep;
            currentPos = nextPos;
        }

        // If no impact found, use the last calculated point
        if (!foundImpact)
        {
            impactPoint = currentPos;
        }

        // Update line renderer
        trajectoryLine.SetPositions(_trajectoryPoints);

        // Update impact indicator position
        if (impactIndicator != null)
        {
            impactIndicator.transform.position = impactPoint + Vector3.up * 0.05f;
        }

        // Update AOE radius indicator
        if (aoeRadiusIndicator != null && explosionRadius > 0)
        {
            aoeRadiusIndicator.transform.position = impactPoint + Vector3.up * 0.05f;
            aoeRadiusIndicator.transform.localScale = new Vector3(explosionRadius * 2f, 0.01f, explosionRadius * 2f);
            aoeRadiusIndicator.SetActive(true);
        }
        else if (aoeRadiusIndicator != null)
        {
            aoeRadiusIndicator.SetActive(false);
        }
    }

    /// <summary>
    /// Calculate trajectory points without rendering (for external use)
    /// </summary>
    public static Vector3[] CalculateTrajectoryPoints(Vector3 startPosition, Vector3 initialVelocity, int segments, float timeStep)
    {
        Vector3[] points = new Vector3[segments];
        Vector3 currentPos = startPosition;
        Vector3 currentVel = initialVelocity;
        Vector3 gravity = Physics.gravity;

        for (int i = 0; i < segments; i++)
        {
            points[i] = currentPos;
            currentVel += gravity * timeStep;
            currentPos += currentVel * timeStep;
        }

        return points;
    }

    /// <summary>
    /// Calculate predicted impact point
    /// </summary>
    public static Vector3 CalculateImpactPoint(Vector3 startPosition, Vector3 initialVelocity, LayerMask groundLayer, float maxTime = 5f)
    {
        Vector3 currentPos = startPosition;
        Vector3 currentVel = initialVelocity;
        Vector3 gravity = Physics.gravity;
        float timeStep = 0.05f;
        float elapsedTime = 0f;

        while (elapsedTime < maxTime)
        {
            Vector3 nextPos = currentPos + currentVel * timeStep + 0.5f * gravity * timeStep * timeStep;

            RaycastHit hit;
            if (Physics.Raycast(currentPos, (nextPos - currentPos).normalized, out hit, (nextPos - currentPos).magnitude, groundLayer))
            {
                return hit.point;
            }

            currentVel += gravity * timeStep;
            currentPos = nextPos;
            elapsedTime += timeStep;
        }

        return currentPos;
    }
}
