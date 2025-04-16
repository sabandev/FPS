using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// GOAP_ACTION_GoTo
/// Inherits GOAP_Action
/// Is the template for all actions that involve moving the AI from point A to point B
/// </summary>
public class GOAP_ACTION_GoTo : GOAP_Action
{
    // Private Variables
    private NavMeshAgent _agent;

    // Private Functions
    private void Start()
    {
        _agent = GetComponent<NavMeshAgent>();
    }

    // Override Functions
    public override bool PreAction()
    {
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
