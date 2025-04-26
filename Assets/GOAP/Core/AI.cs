using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Unity.AI.Navigation;

/// <summary>
/// AIType.
/// Enumeration.
/// Stores all the possible "types" of AI an agent could be.
/// </summary>
// public enum AIType
// {
//     Unspecified,
//     Pathfinder,
//     Idler
// }

/// <summary>
/// AI
/// Stores all of the behavioural logic for an AI Agent.
/// The "brains" of the AI.
/// </summary>
[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(AISensor))]
public class AI: MonoBehaviour
{
    #region Public Properties
    public AIType aiType;
    public GOAP_GoalSet goalSet;

    public NavMeshAgent agent;
    public List<Transform> waypoints;
    public GameObject target;

    public int currentWaypointIndex = 0;

    public float stoppingDistance = 2.0f;

    public bool assignTargetGameObject = false;
    public bool assignWaypoints = false;
    #endregion

    #region Serializable Properties
    [SerializeField] private float walkingSpeed = 4.0f;
    [SerializeField] private float runningSpeed = 7.5f;
    [SerializeField] private float rotationTime = 5.0f;
    [SerializeField] private float angularSpeed = 10.0f;
    [SerializeField] private float jumpHeight = 3.0f;
    [SerializeField] private float jumpDuration = 0.75f;
    [SerializeField] private float ladderClimbDuration = 3.0f;
    [SerializeField] private GOAP_Action currentAction;
    [SerializeField] private List<GOAP_Action> availableActions = new List<GOAP_Action>();
    [SerializeField] private GOAP_Goal currentGoal;
    #endregion

    #region Private Properties
    private GOAP_Planner _planner;
    private GOAP_Planner _validatePlanner;

    private AISensor _sensor;

    private Queue<GOAP_Action> _actionQueue;
    private Queue<GOAP_Action> _validateActionQueue;

    private Dictionary<GOAP_Goal, int> _goals = new Dictionary<GOAP_Goal, int>();

    private Vector3 _navMeshLinkStartPos;

    private Vector3 _navMeshLinkEndPos;

    private bool _isHandlingLink = false;
    private bool _invoked = false;
    private bool _hasSeenPlayer = false;
    #endregion

    // Private Functions
    private void Start()
    {
        // Get sensor
        _sensor = GetComponent<AISensor>();

        // Get available actions
        if (aiType.availableActions.Count <= 0)
            Debug.LogWarning("WARNING: AI Type has no assigned actions. AI cannot operate with no available actions.");
        else
        {
            foreach (GOAP_Action a in aiType.availableActions)
            {
                GOAP_Action instance = Instantiate(a);
                availableActions.Add(instance);
                instance.agent = this;
            }
        }

        // Add pre-assigned goals
        if (goalSet != null)
        {
            foreach (GOAP_Goal g in goalSet.goals)
            {
                if (g.enabled)
                {
                    AddGoal(g.goalName, g.infinte, g.importance);
                }
            }
        }

        // Get/Set NavMesh properties
        agent = GetComponent<NavMeshAgent>();
        agent.autoTraverseOffMeshLink = false;
    }

    private void Update()
    {
        SetNavMeshProperties();

        // if (agent.hasPath)
        //     SpeedControl();

        if (agent.isOnOffMeshLink)
            HandleNavMeshLink();

        if (_sensor.IsInSight(target) && target.activeSelf && !_hasSeenPlayer)
        {
            AddState("seePlayer");
            _hasSeenPlayer = true;
        }

        if (currentAction != null && currentAction.running)
        {
            HandleCurrentAction();
            return;
        }

        if (_planner == null || _actionQueue == null)
            CreatePlan();

        if (_actionQueue != null && _actionQueue.Count == 0 && _goals.Count != 0)
            CompletePlan();

        if (_actionQueue != null && _actionQueue.Count > 0)
            QueueNextAction();
    }

    private void CreatePlan()
    {
        _planner = new GOAP_Planner();

        // Order the goals
        var sortedGoals = from entry in _goals orderby entry.Value descending select entry;

        // Make a plan
        foreach (KeyValuePair<GOAP_Goal, int> g in sortedGoals)
        {
            _actionQueue = _planner.Plan(availableActions, g.Key.goalDictionary, GOAP_World.Instance.worldStatesClass, true);

            if (_actionQueue != null)
            {
                currentGoal = g.Key;
                break;
            }
        }
    }

    private void ValidatePlan()
    {
        _validatePlanner = new GOAP_Planner();

        // Order the goals
        var _sortedGoals = from entry in _goals orderby entry.Value descending select entry;

        // Make a plan
        foreach (KeyValuePair<GOAP_Goal, int> g in _sortedGoals)
        {
            _validateActionQueue = _validatePlanner.Plan(availableActions, g.Key.goalDictionary, GOAP_World.Instance.worldStatesClass, true);

            if (_validateActionQueue != null)
            {
                // We have a new plan to satisfy a new goal
                if (g.Key != currentGoal)
                {
                    currentGoal = g.Key;
                    CompleteAction();
                    _actionQueue = _validateActionQueue;
                    _planner = _validatePlanner;
                }

                break;
            }
        }
    }

    private void CompletePlan()
    {
        if (!currentGoal.infinte)
        {
            _goals.Remove(currentGoal);
            ValidatePlan();
        }

        _planner = null;
    }

    private void HandleCurrentAction()
    {
        if (currentAction.IsComplete(this))
        {
            if (!_invoked)
            {
                _invoked = true;
                Invoke("CompleteAction", currentAction.duration);
            }
        }
        else
        {
            if (!_invoked)
                currentAction.DuringAction(this);
        }
    }

    private void QueueNextAction()
    {
        currentAction = _actionQueue.Dequeue();

        if (currentAction.PreAction(this))
        {
            // Agent specific behaviour here
        }
        else
            _actionQueue = null;
    }

    private void CompleteAction()
    {
        if (currentAction)
        {
            currentAction.running = false;
            currentAction.PostAction(this);
        }
        _invoked = false;
    }

    private void AddGoal(string name, bool infinite, int importance)
    {
        GOAP_Goal newGoal = new GOAP_Goal(name, infinite, importance);
        _goals.Add(newGoal, importance);

        if (!GOAP_World.Instance.goals.Contains(newGoal))
            GOAP_World.Instance.goals.Add(newGoal);
    }

    private void AddState(string stateName, int stateValue = 0)
    {
        GOAP_World.Instance.worldStatesClass.AddState(stateName, stateValue);
        ValidatePlan();
    }

    private void SetNavMeshProperties()
    {
        agent.speed = runningSpeed;
        agent.angularSpeed = angularSpeed * 100f;
        agent.stoppingDistance = stoppingDistance;
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

    // Private Coroutines

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
        while (timer < 1f)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRot, timer / rotationTime);
            //transform.rotation = lookRot;  FAST

            timer += Time.deltaTime;
            yield return null;
        }
    }
}