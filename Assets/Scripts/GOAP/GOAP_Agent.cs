using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using UnityEngine.AI;

/// <summary>
/// GOAP_Goal
/// Template for all goals; they hold a dictionary which allows the raw <string, int> to be referenced in code
/// </summary>
public class GOAP_Goal
{
    // Public Variables
    public Dictionary<string, int> goalDictionary;

    public bool removeAfterCompletion;

    // Constructor
    public GOAP_Goal(string s="goal", bool r=true, int i=1)
    {
        removeAfterCompletion = r;

        goalDictionary = new Dictionary<string, int>();
        goalDictionary.Add(s, i);
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

    public Dictionary<GOAP_Goal, int> goals = new Dictionary<GOAP_Goal, int>();

    public GOAP_Action currentAction;
    public GOAP_Goal currentGoal;

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

            if (agent.hasPath && agent.remainingDistance < stoppingDistance)
            {
                if (!_invoked)
                {
                    Invoke("CompleteAction", currentAction.duration);
                    _invoked = true;
                }
            }
            else if (agent.hasPath && agent.remainingDistance > stoppingDistance)
            {
                MoveToTarget();
                currentAction.DuringAction();
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
                Debug.Log("Goal achieved: " + currentGoal.goalDictionary.Keys);
                goals.Remove(currentGoal);
            }
            
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
            agent.SetDestination(currentAction.target.transform.position);
        }
    }
}