using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Unity.AI.Navigation;

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
    [SerializeField] private float idleTime = 2f;
    [Space(20)]
    [Header("Debug:")]
    [SerializeField] private bool running = false;
    [SerializeField] private int currentWaypointIndex = 0;

    // Variables
    private NavMeshAgent agent;
    private bool isHandlingLink = false;
    private Vector3 _navMeshLinkStartPos;
    private Vector3 _navMeshLinkEndPos;
    private bool _recoveringFromNavMeshLink = false;
    private bool _isIdle = false;

    void Start()
    {
        // Get NavMeshAgent
        agent = GetComponent<NavMeshAgent>();

        // Set stopping distance to custom property
        agent.stoppingDistance = stoppingDistance;

        // Turn off automatic OffMeshLink traversal
        agent.autoTraverseOffMeshLink = false;

        _recoveringFromNavMeshLink = false;
    }

    void Update()
    {
        if (!_isIdle)
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
        if ((agent.hasPath && agent.remainingDistance > stoppingDistance + 4f) && !_recoveringFromNavMeshLink)
        {
            agent.speed = rSpeed;
            running = true;
        }
        else
        {
            agent.speed = wSpeed;
            running = false;
        }

        agent.angularSpeed = rotSpeed * 100f;
    }

    private void PatrolWaypoints()
    {
        // If no waypoints, cancel
        if (waypoints.Count == 0) { return; }

        // If close to waypoint or waypoint is not active, cycle through waypoints
        if (agent.hasPath && agent.remainingDistance <= stoppingDistance)
        {
            _isIdle = true;

            StartCoroutine(IdleTimeDelay(idleTime, () => {
                // // Increment waypoint index
                // currentWaypointIndex ++;

                // // If we are beyond number of waypoints, set it to 0
                // if(currentWaypointIndex >= waypoints.Count)
                //     currentWaypointIndex = 0;
                
                do
                {
                    currentWaypointIndex++;

                    if (currentWaypointIndex >= waypoints.Count)
                        currentWaypointIndex = 0;
                }
                while (!waypoints[currentWaypointIndex].gameObject.activeInHierarchy && currentWaypointIndex < waypoints.Count);

                _isIdle = false;
            }));
        }

        // Set destination to waypoint's position
        if (waypoints[currentWaypointIndex].gameObject.activeInHierarchy)
            agent.SetDestination(waypoints[currentWaypointIndex].position);
    }

    IEnumerator Jump(float height, float duration)
    {
        yield return StartCoroutine(PreNavMeshLinkTraversal());

        // Lerp (jump) between positions
        float normalizedTime = 0.0f;
        while (normalizedTime < 1.0f)
        {
            float yOffset = height * 4.0f * (normalizedTime - normalizedTime * normalizedTime);
            transform.position = Vector3.Lerp(_navMeshLinkStartPos, _navMeshLinkEndPos, normalizedTime) + yOffset * Vector3.up;
            normalizedTime += Time.deltaTime / duration;
            yield return null;
        }

        yield return StartCoroutine(PostNavMeshLinkTraversal());
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

        yield return StartCoroutine(PreNavMeshLinkTraversal());

        // Anchor positions
        Vector3 verticalStart = goingDown
            ? new Vector3(_navMeshLinkEndPos.x, _navMeshLinkStartPos.y, _navMeshLinkEndPos.z)  // Top Y, but bottom XZ
            : new Vector3(_navMeshLinkStartPos.x, _navMeshLinkStartPos.y, _navMeshLinkStartPos.z); // Normal up climb

        Vector3 verticalEnd = goingDown
            ? new Vector3(_navMeshLinkEndPos.x, _navMeshLinkEndPos.y, _navMeshLinkEndPos.z)    // Bottom position
            : new Vector3(_navMeshLinkStartPos.x, _navMeshLinkEndPos.y, _navMeshLinkStartPos.z); // Normal up climb

        // Timings
        float climbDuration = ladderClimbDuration * 0.6f;
        float stepDuration = ladderClimbDuration * 0.4f;
        float timer;

        // If going down, step onto ladder
        if (goingDown)
        {
            timer = 0f;

            while (timer < stepDuration)
            {
                float t = timer / stepDuration;
                transform.position = Vector3.Lerp(transform.position, verticalStart, t);

                timer += Time.deltaTime;
                yield return null;
            }

            // Face the ladder
            yield return StartCoroutine(LookAtNavMeshLink(goingDown));
        }

        // Climbing/descending ladder
        timer = 0f;
        while (timer < climbDuration)
        {
            float t = timer / climbDuration;
            transform.position = Vector3.Lerp(verticalStart, verticalEnd, t);

            timer += Time.deltaTime;
            yield return null;
        }

        // If going up, step onto platform from the ladder
        if (!goingDown)
        {
            timer = 0f;
            Vector3 exitStart = transform.position;
            Vector3 exitEnd = _navMeshLinkEndPos;

            while (timer < stepDuration)
            {
                float t = timer / stepDuration;
                transform.position = Vector3.Lerp(exitStart, exitEnd, t);

                timer += Time.deltaTime;
                yield return null;
            }
        }

        yield return StartCoroutine(PostNavMeshLinkTraversal());
    }

    IEnumerator IdleTimeDelay(float delay, Action callback)
    {
        yield return new WaitForSeconds(delay);
        callback?.Invoke();
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

    IEnumerator LookAtNavMeshLink(bool invertLookDirection)
    {
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

    IEnumerator PreNavMeshLinkTraversal()
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
        yield return StartCoroutine(LookAtNavMeshLink(false));

    }

    IEnumerator PostNavMeshLinkTraversal()
    {
        transform.position = _navMeshLinkEndPos;

        agent.updatePosition = true;
        agent.updateRotation = true;
        agent.isStopped = false;

        agent.CompleteOffMeshLink();

        isHandlingLink = false;

        _recoveringFromNavMeshLink = true;

        yield return new WaitForSeconds(1);

        _recoveringFromNavMeshLink = false;
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

}
