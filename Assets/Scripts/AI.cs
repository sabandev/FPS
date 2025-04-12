
using System.Collections;
using System.Collections.Generic;
using System.Timers;
using Unity.AI.Navigation;
using UnityEditor.ShortcutManagement;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class AI : MonoBehaviour
{
    // Fields
    [Header("Custom Properties")]
    [SerializeField] private List<Transform> waypoints;
    [SerializeField] private int walkingSpeed = 4;
    [SerializeField] private float runningSpeed = 7.5f;
    [SerializeField] private float rotationSpeed = 30f;
    [SerializeField] private float stoppingDistance = 2f;
    [SerializeField] private float jumpHeight = 1.5f;
    [SerializeField] private float jumpDuration = 0.75f;
    [SerializeField] private float ladderClimbDuration = 3f;
    [SerializeField] private Color debugPathColor = Color.blue;
    [Space(20)]
    [Header("Debug:")]
    [SerializeField] private bool running = false;
    [SerializeField] private int currentWaypointIndex = 0;

    // Variables
    private NavMeshAgent agent;
    private bool isHandlingLink = false;
    private Vector3 _navMeshLinkStartPos;
    private Vector3 _navMeshLinkEndPos;
    
    void Start()
    {
        // Get NavMeshAgent
        agent = GetComponent<NavMeshAgent>();

        // Set stopping distance to custom property
        agent.stoppingDistance = stoppingDistance;

        // Turn off automatic OffMeshLink traversal
        agent.autoTraverseOffMeshLink = false;
    }

    void Update()
    {
        PatrolWaypoints();

        SpeedControl(walkingSpeed, runningSpeed, rotationSpeed);

        if (agent.hasPath && !agent.isPathStale)
            DEBUG_DrawLinePath(debugPathColor);

        if (!isHandlingLink && agent.isOnOffMeshLink)
        {
            // Distinguish between NavMeshLink types
            OffMeshLinkData data = agent.currentOffMeshLinkData;
            NavMeshLink link = (NavMeshLink)agent.navMeshOwner;
            int areaType = link.area;

            int ladderArea = NavMesh.GetAreaFromName("Ladder");

            if (areaType == ladderArea)
                StartCoroutine(TraverseLadder());
            else
                StartCoroutine(Jump(jumpHeight, jumpDuration));
        }
    }

    private void SpeedControl(float wSpeed, float rSpeed, float rotSpeed)
    {
        // Speed control
        if ((agent.hasPath && agent.remainingDistance > stoppingDistance + 4f))
        {
            agent.speed = runningSpeed;
            running = true;
        }
        else
        {
            agent.speed = walkingSpeed;
            running = false;
        }

        agent.angularSpeed = rotSpeed * 100f;
    }

    private void PatrolWaypoints()
    {
        // If no waypoints, cancel
        if (waypoints.Count == 0) { return; }

        // If close to waypoint or waypoint is not active, cycle through waypoints
        if ((agent.hasPath && (agent.remainingDistance <= stoppingDistance)) || (waypoints[currentWaypointIndex].gameObject.activeSelf == false))
        {
            // Increment waypoint index
            currentWaypointIndex ++;

            // If we are beyond number of waypoints, set it to 0
            if(currentWaypointIndex >= waypoints.Count)
                currentWaypointIndex = 0;
        }

        // Set destination to waypoint's position
        agent.SetDestination(waypoints[currentWaypointIndex].position);
    }

    private void DEBUG_DrawLinePath(Color c)
    {
        // Get path
        Vector3[] path = agent.path.corners;

        for (int i=0; i < path.Length - 1; i++)
        {
            Debug.DrawLine(path[i], path[i+1], c, 0, true);
        }
    }

    IEnumerator MoveToNavMeshLink(Vector3 target, float duration=0.2f)
    {
        Vector3 initialPos = transform.position;

        float timer = 0f;
        while (timer < duration)
        {
            float t = timer / duration;
            transform.position = Vector3.Lerp(initialPos, target, t);
            timer += Time.deltaTime;
            yield return null;
        }

        transform.position = target;
    }

    IEnumerator Jump(float height, float duration)
    {
        yield return StartCoroutine(PreNavMeshLinkTraversal(false));

        // Lerp (jump) between positions
        float normalizedTime = 0.0f;
        while (normalizedTime < 1.0f)
        {
            float yOffset = height * 4.0f * (normalizedTime - normalizedTime * normalizedTime);
            transform.position = Vector3.Lerp(_navMeshLinkStartPos, _navMeshLinkEndPos, normalizedTime) + yOffset * Vector3.up;
            normalizedTime += Time.deltaTime / duration;
            yield return null;
        }

        PostNavMeshLinkTraversal();
    }

    IEnumerator PreNavMeshLinkTraversal(bool invertLookDirection)
    {
        isHandlingLink = true;

        // Get OffMeshLinkData
        OffMeshLinkData data = agent.currentOffMeshLinkData;

        // Calculate start and end positions of OffMeshLinks
        _navMeshLinkStartPos = data.startPos + Vector3.up * agent.baseOffset;
        _navMeshLinkEndPos = data.endPos + Vector3.up * agent.baseOffset;

        // Disable NavMesh movement
        agent.isStopped = true;
        agent.updatePosition = false;
        agent.updateRotation = false;

        yield return StartCoroutine(MoveToNavMeshLink(_navMeshLinkStartPos, 0.25f));

        // Face direction of startPos
        Vector3 dir;

        if (invertLookDirection == true)
            dir = _navMeshLinkStartPos - _navMeshLinkEndPos;
        else
            dir = _navMeshLinkEndPos - _navMeshLinkStartPos;

        dir.y = 0f;
        Quaternion lookRot = Quaternion.LookRotation(dir);

        // Rotate to face NavMeshLink
        float timer = 0f;
        while (timer < 1f)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRot, Time.deltaTime * rotationSpeed);
            //transform.rotation = lookRot;  FAST

            timer += Time.deltaTime;
            yield return null;
        }
    }

    private void PostNavMeshLinkTraversal()
    {
        transform.position = _navMeshLinkEndPos;

        agent.updatePosition = true;
        agent.updateRotation = true;
        agent.isStopped = false;

        agent.CompleteOffMeshLink();

        isHandlingLink = false;
    }

    IEnumerator TraverseLadder()
    {
        // Get OffMeshLinkData
        OffMeshLinkData data = agent.currentOffMeshLinkData;

        // Calculate start and end positions of OffMeshLinks
        _navMeshLinkStartPos = data.startPos + Vector3.up * agent.baseOffset;
        _navMeshLinkEndPos = data.endPos + Vector3.up * agent.baseOffset;

        // Check if we're going down or up the ladder
        bool goingDown = _navMeshLinkEndPos.y < _navMeshLinkStartPos.y;

        yield return StartCoroutine(PreNavMeshLinkTraversal(goingDown));

        Vector3 climbPosition = _navMeshLinkStartPos;

        float climbDuration = ladderClimbDuration * 0.8f;
        float stepDuration = ladderClimbDuration * 0.2f;

        float timer = 0f;
        while (timer < climbDuration)
        {
            float t = timer / climbDuration;
            climbPosition.y = Mathf.Lerp(_navMeshLinkStartPos.y, _navMeshLinkEndPos.y, t);

            transform.position = climbPosition;

            timer += Time.deltaTime;
            yield return null;
        }

        timer = 0f;
        Vector3 horizontalStart = transform.position;
        Vector3 horizontalEnd = new Vector3(_navMeshLinkEndPos.x, _navMeshLinkEndPos.y, _navMeshLinkEndPos.z);

        while (timer < stepDuration)
        {
            float t = timer / stepDuration;
            transform.position = Vector3.Lerp(horizontalStart, horizontalEnd, t);

            timer += Time.deltaTime;
            yield return null;
        }

        PostNavMeshLinkTraversal();
    }
}
