using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Unity.AI.Navigation;
using JetBrains.Annotations;
using System.Threading;
using System.Linq.Expressions;

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
public class AI: MonoBehaviour
{
    #region Public Properties
    public AIType aiType;

    public GOAP_GoalSet goalSet;

    public GOAP_Goal currentGoal;

    public List<GOAP_Action> availableActions = new List<GOAP_Action>();
    public List<GameObject> currentlyVisibleTargetObjects = new List<GameObject>();
    public List<Transform> waypoints;

    public NavMeshAgent Agent
    {
        get { return _agent; }
    }

    public GameObject target;

    public int currentWaypointIndex = 0;

    public float stoppingDistance = 2.0f;

    public bool canUpdatePath = true;
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
    [SerializeField] private float visionDistance = 30.0f;
    [SerializeField] private float visionAngle = 45.0f;
    [SerializeField] private float visionHeight = 1.5f;
    [SerializeField] private Color visionConeColor = Color.blue;
    [SerializeField] private int visionScanFrequency = 30;
    [SerializeField] private LayerMask visionTargetLayers;
    [SerializeField] private LayerMask visionOcclusionLayers;
    [SerializeField] private bool drawViewCone = true;
    [SerializeField] private bool drawInSightGizmos = true;
    [SerializeField] private float updatePathCooldown = 0.25f;
    [SerializeField] public bool vision = true;
    [SerializeField] public bool hearing = true;
    [SerializeField] private float hearingRange = 12.5f;
    [SerializeField] private bool drawHearingRadiusGizmo = true;
    [SerializeField] private Color hearingRadiusColor = Color.red;
    #endregion

    #region Private Properties
    private GOAP_Planner _planner;
    private GOAP_Planner _validatePlanner;

    private Queue<GOAP_Action> _actionQueue;
    private Queue<GOAP_Action> _validateActionQueue;

    private Dictionary<GOAP_Goal, int> _goals = new Dictionary<GOAP_Goal, int>();

    private NavMeshAgent _agent;

    private Vector3 _navMeshLinkStartPos;
    private Vector3 _navMeshLinkEndPos;

    private Collider[] _visionColliders = new Collider[50];
    
    private Mesh _mesh;

    private int _visionScanCount;

    private float _visionScanInterval;
    private float _visionScanTimer;
    private float _nextContinueActionTime = 0.0f;

    private bool _isHandlingLink = false;
    private bool _completeInvoked = false;
    private bool _hasSeenPlayer = false;
    #endregion

    // Private Functions
    private void Start()
    {
        // Get sensor
        SetScanVisionTimer();
        
        // Get available actions
        if (aiType == null)
        {
            Debug.LogError($"ERROR: AI Type not set on Agent: {gameObject.name}");
            return;
        }

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
        _agent = GetComponent<NavMeshAgent>();

        SetNavMeshProperties();
    }

    private void Update()
    {
        // Optional DuringAction() call cooldown
        if (Time.time >= _nextContinueActionTime)
        {
            canUpdatePath = true;
            _nextContinueActionTime = Time.time + updatePathCooldown;
        }
        else
            canUpdatePath = false;

        ScanVisionTimer();

        // if (agent.hasPath)
        //     SpeedControl();

        if (Agent != null && Agent.isOnOffMeshLink)
            HandleNavMeshLink();

        if (IsInSight(target) && target.activeSelf && !_hasSeenPlayer)
        {
            Debug.Log("see");
            AddState("seePlayer");
            _hasSeenPlayer = true;
        }

        if (target.activeSelf && !_hasSeenPlayer)
            ListenForSounds();

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

    private void OnValidate()
    {
        if (vision)
        {
            // Create the mesh and reset the vision scan timer
            _mesh = ViewConeMesh();
            _visionScanInterval = 1.0f / visionScanFrequency;
        }
    }

    private void OnDrawGizmos()
    {
        // Draw vision cone
        if (_mesh && drawViewCone && vision)
        {
            Gizmos.color = visionConeColor;
            Gizmos.DrawMesh(_mesh, new Vector3(transform.position.x, transform.position.y - 0.5f, transform.position.z), transform.rotation);

            // Gizmos.DrawWireSphere(transform.position, visionDistance);
        }

        // In sensor / sight
        if (drawInSightGizmos && vision)
        {
            ScanVision();

            Gizmos.color = Color.red;
            for (int i = 0; i < _visionScanCount; i++)
            {
                Vector3 effectiveCenter = _visionColliders[i].transform.position;
                effectiveCenter.y += 1f;
                Gizmos.DrawSphere(effectiveCenter, 0.5f);
            }

            // In sight
            Gizmos.color = Color.green;
            foreach (var obj in currentlyVisibleTargetObjects)
            {
                Vector3 effectiveCenter = obj.transform.position;
                effectiveCenter.y += 1f;
                Gizmos.DrawSphere(effectiveCenter, 0.5f);
            }
        }

        if (drawHearingRadiusGizmo && hearing)
        {
            Gizmos.color = hearingRadiusColor;
            Gizmos.DrawWireSphere(transform.position, hearingRange);
        }

        // if (GOAP_World.Instance == null) { return; } 

        // Gizmos.color = Color.red;
        // foreach (SoundEvent sound in GOAP_World.Instance.GetRecentSounds())
        // {
        //     Gizmos.DrawSphere(sound.position, 1.0f);
        // }
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
            if (!_completeInvoked)
            {
                _completeInvoked = true;
                Invoke("CompleteAction", currentAction.duration);
            }
        }
        else
        {
            if (!_completeInvoked)
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
        _completeInvoked = false;
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
        Agent.speed = runningSpeed;
        Agent.angularSpeed = angularSpeed * 100f;
        Agent.stoppingDistance = stoppingDistance;
        Agent.autoTraverseOffMeshLink = false;
    }

    private void SpeedControl()
    {
        if (Agent.remainingDistance > stoppingDistance + 4f)
            Agent.speed = runningSpeed;
        else
            Agent.speed = walkingSpeed;
    }

    private void HandleNavMeshLink()
    {
        if (!_isHandlingLink)
        {
            // Distinguish between NavMeshLink types
            OffMeshLinkData data = Agent.currentOffMeshLinkData;
            NavMeshLink link = (NavMeshLink)Agent.navMeshOwner;
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

    private void ScanVision()
    {
        _visionScanCount = Physics.OverlapSphereNonAlloc(transform.position, visionDistance, _visionColliders, visionTargetLayers, QueryTriggerInteraction.Collide);
        currentlyVisibleTargetObjects.Clear();

        for (int i = 0; i < _visionScanCount; i++)
        {
            GameObject obj = _visionColliders[i].gameObject;

            if (IsInSight(obj))
                currentlyVisibleTargetObjects.Add(obj);
        }
    }

    private Mesh ViewConeMesh()
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

    private bool IsInSight(GameObject obj)
    {
        if (!vision) { return false; }

        Vector3 origin = transform.position;
        Vector3 destination = obj.transform.position;

        destination.y += 1f;

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

    private void SetScanVisionTimer()
    {
        if (!vision) { return; }

        _visionScanInterval = 1.0f / visionScanFrequency;
    }

    private void ScanVisionTimer()
    {
        if (!vision) { return; }

        _visionScanTimer -= Time.deltaTime;

        if (_visionScanTimer < 0.0f)
        {
            _visionScanTimer += _visionScanInterval;
            ScanVision();
        }
    }

    private void ListenForSounds()
    {
        if (!hearing) { return; }

        foreach (SoundEvent sound in GOAP_World.ActiveSounds)
        {
            float effectiveRadius = sound.volume;
            float distance = Vector3.Distance(transform.position, sound.position);

            if (distance <= Mathf.Min(effectiveRadius, hearingRange))
            {
                if (sound.type == SoundType.Player)
                {
                    Debug.Log($"{name} heard the player at distance {distance}");

                    AddState("seePlayer");
                    _hasSeenPlayer = true;

                    GOAP_World.ClearActiveSounds();
                    return;
                }
            }
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
        OffMeshLinkData data = Agent.currentOffMeshLinkData;

        // Calculate start and end positions of OffMeshLinks
        _navMeshLinkStartPos = data.startPos + Vector3.up * Agent.baseOffset;
        _navMeshLinkEndPos = data.endPos + Vector3.up * Agent.baseOffset;

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
        OffMeshLinkData data = Agent.currentOffMeshLinkData;

        // Calculate start and end positions of OffMeshLinks
        _navMeshLinkStartPos = data.startPos + Vector3.up * Agent.baseOffset;
        _navMeshLinkEndPos = data.endPos + Vector3.up * Agent.baseOffset;

        // Disable NavMesh movement
        Agent.updatePosition = false;
        Agent.updateRotation = false;

        yield return StartCoroutine(MoveToNavMeshLink(_navMeshLinkStartPos, 0.25f));
        yield return StartCoroutine(LookAt(_navMeshLinkEndPos - _navMeshLinkStartPos));

    }

    private IEnumerator PostNavMeshLinkTraversal()
    {
        // Snap to end point
        transform.position = _navMeshLinkEndPos;

        // Reset velocity
        Agent.velocity = Vector3.zero;

        // Face destination
        Vector3 destination = Agent.destination;

        if (Agent.path.corners.Length > 1)
            destination = Agent.path.corners[1] - transform.position;

        yield return StartCoroutine(LookAt(destination));

        // Re-enable NavMeshAgent updating
        Agent.updateRotation = true;
        Agent.updatePosition = true;

        _isHandlingLink = false;

        Agent.CompleteOffMeshLink();
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