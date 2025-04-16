using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// GOAP_ACTION_GoTo
/// Inherits GOAP_Action
/// Is the template for all actions that involve moving the AI from point A to point B
/// </summary>
public class GOAP_ACTION_GoTo : GOAP_Action
{
    // Protected Variables
    protected NavMeshAgent _agent;

    // Private Functions
    private void MoveToTarget()
    {
        // If we have a target, set a destination to the target
        if (target == null && targetTag != string.Empty)
            target = GameObject.FindWithTag(targetTag);

        if (target != null && _agent != null)
        {
            running = true;
            _agent.SetDestination(target.transform.position);
        }
    }

    // Override Functions
    protected override void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();

        base.Awake();
    }

    public override bool PreAction()
    {
        // Debug.Log("Called GoTo PreAction");
        MoveToTarget();
        return true;
    }

    public override bool DuringAction()
    {
        MoveToTarget();
        return true;
    }

    public override bool PostAction()
    {
        return true;
    }

    public override bool IsComplete()
    {
        if (_agent != null)
        {
            if (_agent.hasPath && _agent.remainingDistance < _agent.stoppingDistance)
                return true;
        }
        else
        {
            if (Vector3.Distance(transform.position, target.transform.position) < 3.0f)
                return true;
        }

        return false;
    }


}
