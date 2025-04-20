using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using UnityEngine.AI;

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
public class GOAP_Agent : MonoBehaviour
{
    // Inspector Variables
    public AIType aiType = AIType.Pathfinder;

    public ActionManager actionManager;

    public float walkingSpeed = 4.0f;
    public float runningSpeed = 7.5f;
    public float rotationSpeed = 10.0f;
    public float stoppingDistance = 2.0f;

    // Public Variables
    public NavMeshAgent agent;

    public List<GOAP_Action> actions = new List<GOAP_Action>();

    public Dictionary<GOAP_Goal, int> goals = new Dictionary<GOAP_Goal, int>();

    public GOAP_Action currentAction;
    public GOAP_Goal currentGoal;

    public GameObject target;

    public List<Transform> waypoints;
    public int currentWaypointIndex = 0;

    // Private Variables
    private GOAP_Planner _planner;

    private Queue<GOAP_Action> _actionQueue;

    private bool _invoked = false;

    // Protected Functions
    protected virtual void Awake()
    {
        // Get actions
        if (actionManager != null)
        {
            foreach (GOAP_Action a in actionManager.GetActions(this))
            {
                if (a == null)
                    continue;

                GOAP_Action actionInstance = Instantiate(a);
                a.agent = this;
                actions.Add(actionInstance);
            }
        }
        else
            Debug.LogWarning("WARNING: ActionManager not assigned in agent");

        // Set NavMesh properties
        agent = GetComponent<NavMeshAgent>();

        agent.speed = runningSpeed;
        agent.angularSpeed = rotationSpeed * 100f;
        agent.stoppingDistance = stoppingDistance;
    }

    // Private Functions
    void LateUpdate()
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
            var sortedGoals = from entry in goals orderby entry.Value descending select entry;

            // Make a plan
            foreach (KeyValuePair<GOAP_Goal, int> g in sortedGoals)
            {
                _actionQueue = _planner.Plan(actions, g.Key.goalDictionary, null);

                if (_actionQueue != null)
                {
                    currentGoal = g.Key;
                    break;
                }
            }
        }

        // Check if the AI has completed its plan
        if (_actionQueue != null && _actionQueue.Count == 0 && goals.Count != 0)
        {
            if (currentGoal.removeAfterCompletion)
            {
                goals.Remove(currentGoal);
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

    private void CompleteAction()
    {
        currentAction.running = false;
        currentAction.PostAction(this);
        _invoked = false;
    }
}