using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// DEBUG_ShowAIPath
/// A debugging script that will render a line to show the AI's path in the scene view
/// </summary>
[RequireComponent(typeof(LineRenderer), typeof(NavMeshAgent))]
public class DEBUG_ShowAIPath : MonoBehaviour
{
    // Inspector Variables
    [SerializeField] private Color debugPathColor = Color.blue;
    [SerializeField] private float debugLineWidth = 0.1f;
    [SerializeField] private float debugLineElevation = 0.5f;

    // Private Variables
    private LineRenderer lineRenderer;

    private NavMeshAgent agent;

    // Private Functions
    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.widthMultiplier = debugLineWidth;
        lineRenderer.startColor = debugPathColor;
        lineRenderer.endColor = debugPathColor;
        lineRenderer.positionCount = 0;

        lineRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        lineRenderer.receiveShadows = false;
        lineRenderer.useWorldSpace = true;
    }

    private void Update()
    {
        if (agent.hasPath)
            DEBUG_DrawLinePath();
        else
            lineRenderer.enabled = false;
    }

    private void DEBUG_DrawLinePath()
    {
        // if (!agent.hasPath) { return; }
        lineRenderer.enabled = true;

        Vector3[] agentPath = agent.path.corners;

        List<Vector3> originalPath = new List<Vector3>();
        foreach (Vector3 p in agentPath)
        {
            Vector3 newP = new Vector3(p.x, p.y + debugLineElevation, p.z);
            originalPath.Add(newP);
        }

        Vector3[] elevatedPath = originalPath.ToArray();

        lineRenderer.positionCount = elevatedPath.Length;
        lineRenderer.SetPositions(elevatedPath);
    }
}
