using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using Unity.AI.Navigation;

public class Pathfinder : GOAP_Agent
{
    // Inspector Variables
    [SerializeField] private float jumpHeight = 3.0f;
    [SerializeField] private float jumpDuration = 0.75f;
    [SerializeField] private float ladderClimbDuration = 3.0f;

    // Private Variables
    private Vector3 _navMeshLinkStartPos;
    private Vector3 _navMeshLinkEndPos;

    private bool _isHandlingLink = false;

    // New Functions
    private void Start()
    {
        // GOAP_Goal patrolWaypointsGoal = new GOAP_Goal("PatrolWaypoints", false);
        // goals.Add(patrolWaypointsGoal, 3);

        // GOAP_Goal g2 = new GOAP_Goal("PatrolRandom", false);
        // goals.Add(g2, 10);

        GOAP_Goal inRoomBGoal = new GOAP_Goal("inRoomB", true);
        goals.Add(inRoomBGoal, 10);

        GOAP_Goal idleGoal = new GOAP_Goal("Idle", false);
        goals.Add(idleGoal, 1);

        // OffMeshLinks
        agent.autoTraverseOffMeshLink = false;
    }

    // Private Functions
    private void Update()
    {
        if (agent.hasPath)
            SpeedControl();

        if (agent.isOnOffMeshLink)
            HandleNavMeshLink();
    }

    private void HandleNavMeshLink()
    {
        if (!_isHandlingLink)
        {
            // Distinguish between NavMeshLink types
            OffMeshLinkData data = agent.currentOffMeshLinkData;
            NavMeshLink link = (NavMeshLink)agent.navMeshOwner;
            int areaType = link.area;

            // Get the ladder's index
            int ladderArea = NavMesh.GetAreaFromName("Ladder");

            if (areaType == ladderArea)
                StartCoroutine(TraverseLadder());
            else
                StartCoroutine(Jump(jumpHeight, jumpDuration));
        }
    }

    private IEnumerator Jump(float height, float duration)
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

    private IEnumerator TraverseLadder()
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
        float climbDuration = ladderClimbDuration;
        float stepDuration = walkingSpeed * 0.1f;
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
            yield return StartCoroutine(LookAt(_navMeshLinkStartPos - _navMeshLinkEndPos));
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

    private void SpeedControl()
    {
        if (agent.remainingDistance > stoppingDistance + 4f)
            agent.speed = runningSpeed;
        else
            agent.speed = walkingSpeed;
    }

    private IEnumerator PreNavMeshLinkTraversal()
    {
        _isHandlingLink = true;

        // Get OffMeshLinkData
        OffMeshLinkData data = agent.currentOffMeshLinkData;

        // Calculate start and end positions of OffMeshLinks
        _navMeshLinkStartPos = data.startPos + Vector3.up * agent.baseOffset;
        _navMeshLinkEndPos = data.endPos + Vector3.up * agent.baseOffset;

        // Disable NavMesh movement
        agent.updatePosition = false;
        agent.updateRotation = false;

        yield return StartCoroutine(MoveToNavMeshLink(_navMeshLinkStartPos, 0.25f));
        yield return StartCoroutine(LookAt(_navMeshLinkEndPos - _navMeshLinkStartPos));

    }

    private IEnumerator PostNavMeshLinkTraversal()
    {
        // Snap to end point
        transform.position = _navMeshLinkEndPos;

        // Reset velocity
        agent.velocity = Vector3.zero;

        // Face destination
        Vector3 destination = agent.path.corners[1] - transform.position;

        yield return StartCoroutine(LookAt(destination));

        // Re-enable NavMeshAgent updating
        agent.updateRotation = true;
        agent.updatePosition = true;

        _isHandlingLink = false;

        agent.CompleteOffMeshLink();
    }

    private IEnumerator MoveToNavMeshLink(Vector3 target, float duration = 0.2f)
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

    private IEnumerator LookAt(Vector3 dir)
    {
        dir.y = 0f;
        Quaternion lookRot = Quaternion.LookRotation(dir);

        // Rotate to face NavMeshLink
        float timer = 0f;
        float duration = rotationSpeed * 0.1f;
        while (timer < 1f)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRot, timer / duration);
            //transform.rotation = lookRot;  FAST

            timer += Time.deltaTime;
            yield return null;
        }
    }
}