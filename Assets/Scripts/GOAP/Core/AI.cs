using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Unity.AI.Navigation;

/// <summary>
/// AIType
/// Enumeration
/// Stores all the possible "types" of AI an agent could be
/// </summary>
public enum AIType
{
    Unspecified,
    Pathfinder,
    Idler
}

/// <summary>
/// GOAP_Agent
/// A template for all AI agents
/// </summary>
[RequireComponent(typeof(NavMeshAgent))]
public class AI: MonoBehaviour
{
    // Public Variables
    public AIType aiType = AIType.Pathfinder;

    public ActionManager actionManager;

    public float walkingSpeed = 4.0f;
    public float runningSpeed = 7.5f;
    public float rotationSpeed = 10.0f;
    public float stoppingDistance = 2.0f;

    public NavMeshAgent agent;

    public List<GOAP_Action> availableActions = new List<GOAP_Action>();
    public List<GOAP_Goal> goals = new List<GOAP_Goal>();

    public Dictionary<GOAP_Goal, int> goalsDictionary = new Dictionary<GOAP_Goal, int>();

    public GOAP_Action currentAction;
    public GOAP_Goal currentGoal;

    public GameObject target;

    public List<Transform> waypoints;
    public int currentWaypointIndex = 0;

    // Inspector Variables
    [SerializeField] private float jumpHeight = 3.0f;
    [SerializeField] private float jumpDuration = 0.75f;
    [SerializeField] private float ladderClimbDuration = 3.0f;

    // Private Variables
    private GOAP_Planner _planner;

    private Queue<GOAP_Action> _actionQueue;

    private Vector3 _navMeshLinkStartPos;
    private Vector3 _navMeshLinkEndPos;

    private bool _isHandlingLink = false;
    private bool _invoked = false;

    // Protected Functions
    protected virtual void Awake()
    {
        // Get ActionManager
        actionManager = FindAnyObjectByType<ActionManager>().GetComponent<ActionManager>();

        // Get actions
        if (actionManager != null)
        {
            foreach (GOAP_Action a in actionManager.GetActions(this))
            {
                if (a == null)
                    continue;

                GOAP_Action actionInstance = Instantiate(a);
                a.agent = this;
                availableActions.Add(actionInstance);
            }
        }
        else
            Debug.LogWarning("WARNING: ActionManager not assigned in agent");

        // Add pre-assigned goals
        if (goals.Count != 0)
        {
            foreach (GOAP_Goal g in goals)
                AddGoal(g.name, g.removeAfterCompletion, g.importance);
        }

        // Set NavMesh properties
        agent = GetComponent<NavMeshAgent>();

        agent.speed = runningSpeed;
        agent.angularSpeed = rotationSpeed * 100f;
        agent.stoppingDistance = stoppingDistance;
        agent.autoTraverseOffMeshLink = false;
    }

    protected virtual void AddGoal(string name, bool removeAfterCompletion, int importance)
    {
        GOAP_Goal newGoal = new GOAP_Goal(name, removeAfterCompletion);
        goalsDictionary.Add(newGoal, importance);

        if (!GOAP_World.Instance.goals.Contains(name))
            GOAP_World.Instance.goals.Add(name);
    }

    // Private Functions
    private void LateUpdate()
    {
        // Check if the AI is performing an action and complete the action if it is done
        // If it is not done, return out of LateUpdate
        if (currentAction != null && currentAction.running)
        {
            if (currentAction.IsComplete(this))
            {
                if (!_invoked)
                {
                    Invoke("CompleteAction", currentAction.duration);
                    _invoked = true;
                }
            }
            else
            {
                currentAction.DuringAction(this);
            }

            return;
        }

        // Check if we have a plan
        // If not, we make one
        if (_planner == null || _actionQueue == null)
        {
            _planner = new GOAP_Planner();

            // Order the goals
            var sortedGoals = from entry in goalsDictionary orderby entry.Value descending select entry;

            // Make a plan
            foreach (KeyValuePair<GOAP_Goal, int> g in sortedGoals)
            {
                _actionQueue = _planner.Plan(availableActions, g.Key.goalDictionary, null);

                if (_actionQueue != null)
                {
                    currentGoal = g.Key;
                    break;
                }
            }
        }

        // Check if the AI has completed its plan
        if (_actionQueue != null && _actionQueue.Count == 0 && goalsDictionary.Count != 0)
        {
            if (currentGoal.removeAfterCompletion)
            {
                goalsDictionary.Remove(currentGoal);
            }
            
            _planner = null;
        }

        if (_actionQueue != null && _actionQueue.Count > 0)
        {
            currentAction = _actionQueue.Dequeue();

            if (currentAction.PreAction(this))
            {
                // Agent specific behaviour here
            }
            else
                _actionQueue = null;
        }
    }

    private void Update()
    {
        if (agent.hasPath)
            SpeedControl();

        if (agent.isOnOffMeshLink)
            HandleNavMeshLink();
    }

    private void SpeedControl()
    {
        if (agent.remainingDistance > stoppingDistance + 4f)
            agent.speed = runningSpeed;
        else
            agent.speed = walkingSpeed;
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
            int doorArea = NavMesh.GetAreaFromName("Door");

            if (areaType == ladderArea)
                StartCoroutine(TraverseLadder());
            else if (areaType == doorArea)
            {
                SlideUpDoor door = link.gameObject.GetComponentInParent<SlideUpDoor>();

                StartCoroutine(TraverseDoor(door));
            }
            else
                StartCoroutine(TraverseJump(jumpHeight, jumpDuration));
        }
    }

    private void CompleteAction()
    {
        currentAction.running = false;
        currentAction.PostAction(this);
        _invoked = false;
    }

    private IEnumerator TraverseJump(float height, float duration)
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

    private IEnumerator TraverseDoor(SlideUpDoor door)
    {
        if (door != null)
        {
            yield return PreNavMeshLinkTraversal();

            // Open the door
            door.OpenDoor();

            yield return new WaitForSeconds(door.duration);

            // Walk through the door
            float distance = Vector3.Distance(_navMeshLinkStartPos, _navMeshLinkEndPos);
            float duration = distance / walkingSpeed;
            float time = 0.0f;
            while (time < duration)
            {
                time += Time.deltaTime;
                float t = time / duration;
                transform.position = Vector3.Lerp(_navMeshLinkStartPos, _navMeshLinkEndPos, t);
                yield return null;
            }

            yield return StartCoroutine(PostNavMeshLinkTraversal());
        }
        else
            Debug.LogWarning("WARNING: Parent of OffMeshLink does not have a door component");
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
        Vector3 destination = agent.destination;

        if (agent.path.corners.Length > 1)
            destination = agent.path.corners[1] - transform.position;

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