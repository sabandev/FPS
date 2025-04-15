using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using UnityEngine.PlayerLoop;
using Unity.VisualScripting;
using UnityEngine.AI;

/// <summary>
/// GOAP_SubGoal
/// Template for all sub goals
/// Every goal will be made up of sub goals
/// </summary>
public class GOAP_SubGoal
{
    // Public Variables
    public Dictionary<string, int> subGoals;

    public bool removeAfterCompletion;

    // Constructor
    public GOAP_SubGoal(string s, int importance, bool r)
    {
        subGoals = new Dictionary<string, int>();
        subGoals.Add(s, importance);
        removeAfterCompletion = r;
    }
}

[RequireComponent(typeof(NavMeshAgent))]
public class GOAP_Agent : MonoBehaviour
{
    // Inspector Variables
    public float walkingSpeed = 4.0f;
    public float runningSpeed = 7.5f;
    public float rotationSpeed = 10.0f;
    public float stoppingDistance = 2.0f;

    // Public Variables
    public NavMeshAgent agent;

    public List<GOAP_Action> actions = new List<GOAP_Action>();

    public Dictionary<GOAP_SubGoal, int> goals = new Dictionary<GOAP_SubGoal, int>();

    public GOAP_Action currentAction;
    public GOAP_SubGoal currentGoal;

    // Private Variables
    private GOAP_Planner _planner;

    private Queue<GOAP_Action> _actionQueue;

    private bool _invoked = false;

    // Protected Functions
    protected virtual void Start()
    {
        GOAP_Action[] acts = GetComponents<GOAP_Action>();

        foreach (GOAP_Action a in acts)
            actions.Add(a);

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
            // NOTE: implement a check for if an action is complete, dont just rely on NavMesh
            float distanceToTarget = Vector3.Distance(currentAction.target.transform.position, transform.position);

            if (currentAction.agent.hasPath && distanceToTarget < stoppingDistance)
            {
                if (!_invoked)
                {
                    Invoke("CompleteAction", currentAction.duration);
                    _invoked = true;
                }
            }
            else if (currentAction.agent.hasPath && distanceToTarget > stoppingDistance)
                MoveToTarget();

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
            foreach (KeyValuePair<GOAP_SubGoal, int> g in sortedGoals)
            {
                _actionQueue = _planner.Plan(actions, g.Key.subGoals, null);

                if (_actionQueue != null)
                {
                    currentGoal = g.Key;
                    break;
                }
            }
        }

        // Check if the AI has completed its plan
        if (_actionQueue != null && _actionQueue.Count == 0)
        {
            if (currentGoal.removeAfterCompletion)
                goals.Remove(currentGoal);
            
            _planner = null;
        }

        if (_actionQueue != null && _actionQueue.Count > 0)
        {
            currentAction = _actionQueue.Dequeue();

            if (currentAction.PreAction())
            {
                MoveToTarget();
            }
            else
                _actionQueue = null;
        }
    }

    private void CompleteAction()
    {
        currentAction.running = false;
        currentAction.PostAction();
        _invoked = false;
    }
    
    private void MoveToTarget()
    {
        // NOTE: implement a check for if the action has a target or a target tag

        // If we have a target, set a destination to the target
        if (currentAction.target == null && currentAction.targetTag != string.Empty)
            currentAction.target = GameObject.FindWithTag(currentAction.targetTag);

        if (currentAction.target != null)
        {
            currentAction.running = true;
            currentAction.agent.SetDestination(currentAction.target.transform.position);
        }
    }
}