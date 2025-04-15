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
    private bool _recoveringFromNavMeshLink = false;

    // New Functions
    new void Start()
    {
        base.Start();

        GOAP_SubGoal g1 = new GOAP_SubGoal("PatrolWaypoints", 1, false);
        goals.Add(g1, 3);

        // OffMeshLinks
        agent.autoTraverseOffMeshLink = false;
        _recoveringFromNavMeshLink = false;
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
            yield return StartCoroutine(LookAtNavMeshLink(true));
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

    IEnumerator PreNavMeshLinkTraversal()
    {
        _isHandlingLink = true;

        // Get OffMeshLinkData
        OffMeshLinkData data = agent.currentOffMeshLinkData;

        // Calculate start and end positions of OffMeshLinks
        _navMeshLinkStartPos = data.startPos + Vector3.up * agent.baseOffset;
        _navMeshLinkEndPos = data.endPos + Vector3.up * agent.baseOffset;

        // Disable NavMesh movement
        agent.isStopped = true;
        agent.updatePosition = false;
        agent.updateRotation = false;

        _recoveringFromNavMeshLink = true;

        yield return StartCoroutine(MoveToNavMeshLink(_navMeshLinkStartPos, 0.25f));
        yield return StartCoroutine(LookAtNavMeshLink(false));

    }

    IEnumerator PostNavMeshLinkTraversal()
    {
        transform.position = _navMeshLinkEndPos;

        agent.updateRotation = true;
        agent.isStopped = false;

        yield return new WaitForSeconds(0.5f);

        agent.updatePosition = true;

        agent.CompleteOffMeshLink();

        _isHandlingLink = false;

        // Speed handicap after a NavMeshLink
        yield return new WaitForSeconds(1);

        _recoveringFromNavMeshLink = false;
    }

    IEnumerator MoveToNavMeshLink(Vector3 target, float duration = 0.2f)
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
}
