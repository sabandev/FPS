using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// AISensor.
/// A required component of the AI class.
/// Grants the AI agent its senses and the ability to observe its environment.
/// </summary>
[ExecuteInEditMode]
public class AISensor : MonoBehaviour
{
    #region Serialized Properties
    [SerializeField] private float visionDistance = 30.0f;
    [SerializeField] private float visionAngle = 45.0f;
    [SerializeField] private float visionHeight = 1.5f;
    [SerializeField] private Color visionConeColor = Color.black;
    [SerializeField] private int visionScanFrequency = 30;
    [SerializeField] private LayerMask visionTargetLayers;
    [SerializeField] private LayerMask visionOcclusionLayers;
    [SerializeField] private List<GameObject> currentlyVisibleTargetObjects = new List<GameObject>();
    [SerializeField] private bool drawViewCone = true;
    [SerializeField] private bool drawInSightGizmos = true;
    #endregion

    #region Private Properties
    private Collider[] colliders = new Collider[50];

    private Mesh mesh;

    private int count;
    private float scanInterval;
    private float scanTimer;
    #endregion

    // Private Functions
    private void Start()
    {
        scanInterval = 1.0f / visionScanFrequency;
    }

    private void Update()
    {
        scanTimer -= Time.deltaTime;

        if (scanTimer < 0.0f)
        {
            scanTimer += scanInterval;
            Scan();
        }
    }

    private void Scan()
    {
        count = Physics.OverlapSphereNonAlloc(transform.position, visionDistance, colliders, visionTargetLayers, QueryTriggerInteraction.Collide);
        currentlyVisibleTargetObjects.Clear();

        for (int i = 0; i < count; i++)
        {
            GameObject obj = colliders[i].gameObject;

            if (IsInSight(obj))
                currentlyVisibleTargetObjects.Add(obj);
        }
    }

    private Mesh WedgeMesh()
    {
        Mesh mesh = new Mesh();

        int segments = 10;
        int numTriangles = (segments * 4) + 2 + 2;
        int numVertices = numTriangles * 3;

        Vector3[] vertices = new Vector3[numVertices];
        int[] triangles = new int[numVertices];

        #region Calculate Mesh Vectors
        // Bottom vectors
        Vector3 bottomCenter = Vector3.zero;
        Vector3 bottomLeft = Quaternion.Euler(0, -visionAngle, 0) * Vector3.forward * visionDistance;
        Vector3 bottomRight = Quaternion.Euler(0, visionAngle, 0) * Vector3.forward * visionDistance;

        // Top vectors
        Vector3 topCenter = bottomCenter + Vector3.up * visionHeight;
        Vector3 topLeft = bottomLeft + Vector3.up * visionHeight;
        Vector3 topRight = bottomRight + Vector3.up * visionHeight;
        #endregion

        #region Draw Mesh
        int vert = 0;

        // Left side
        vertices[vert++] = bottomCenter;
        vertices[vert++] = bottomLeft;
        vertices[vert++] = topLeft;

        vertices[vert++] = topLeft;
        vertices[vert++] = topCenter;
        vertices[vert++] = bottomCenter;

        // Right side
        vertices[vert++] = bottomCenter;
        vertices[vert++] = topCenter;
        vertices[vert++] = topRight;

        vertices[vert++] = topRight;
        vertices[vert++] = bottomRight;
        vertices[vert++] = bottomCenter;

        float currentAngle = -visionAngle;
        float deltaAngle = (visionAngle * 2) / segments;

        for (int i = 0; i < segments; i++)
        {
            // Bottom vectors
            bottomLeft = Quaternion.Euler(0, currentAngle, 0) * Vector3.forward * visionDistance;
            bottomRight = Quaternion.Euler(0, currentAngle + deltaAngle, 0) * Vector3.forward * visionDistance;

            // Top vectors
            topLeft = bottomLeft + Vector3.up * visionHeight;
            topRight = bottomRight + Vector3.up * visionHeight;

            currentAngle += deltaAngle;

            // Far side
            vertices[vert++] = bottomLeft;
            vertices[vert++] = bottomRight;
            vertices[vert++] = topRight;

            vertices[vert++] = topRight;
            vertices[vert++] = topLeft;
            vertices[vert++] = bottomLeft;

            // Top
            vertices[vert++] = topCenter;
            vertices[vert++] = topLeft;
            vertices[vert++] = topRight;

            // Bottom
            vertices[vert++] = bottomCenter;
            vertices[vert++] = bottomRight;
            vertices[vert++] = bottomLeft;
        }
        
        for (int i = 0; i < numVertices; i++)
        {
            triangles[i] = i;
        }
        #endregion
        
        // Set mesh triangles and vertices that we calculated
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();

        return mesh;
    }

    private void OnValidate()
    {
        mesh = WedgeMesh();
        scanInterval = 1.0f / visionScanFrequency;
    }

    private void OnDrawGizmos()
    {
        if (mesh && drawViewCone)
        {
            Gizmos.color = visionConeColor;
            Gizmos.DrawMesh(mesh, new Vector3(transform.position.x, transform.position.y - 0.5f, transform.position.z), transform.rotation);
        }

        // In sensor / sight
        if (drawInSightGizmos)
        {
            Gizmos.DrawWireSphere(transform.position, visionDistance);

            Gizmos.color = Color.red;
            for (int i = 0; i < count; i++)
            {
                Gizmos.DrawSphere(colliders[i].transform.position, 0.5f);
            }

            // In sight
            Gizmos.color = Color.green;
            foreach (var obj in currentlyVisibleTargetObjects)
            {
                Gizmos.DrawSphere(obj.transform.position, 0.5f);
            }
        }
    }

    // Public Functions
    public bool IsInSight(GameObject obj)
    {
        Vector3 origin = transform.position;
        Vector3 destination = obj.transform.position;
        Vector3 direction = destination - origin;

        if (direction.y < 0 || direction.y > visionHeight)
            return false;

        direction.y = 0;
        float deltaAngle = Vector3.Angle(direction, transform.forward);
        if (deltaAngle > visionAngle)
            return false;

        origin.y += visionHeight / 2;
        destination.y = origin.y;

        if (Physics.Linecast(origin, destination, visionOcclusionLayers))
            return false;

        return true;
    }
}
