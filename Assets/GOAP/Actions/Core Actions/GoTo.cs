using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// GOAP_ACTION_GoTo
/// Inherits GOAP_Action
/// Is the template for all actions that involve moving the AI from point A to point B
/// </summary>
public class GoTo : GOAP_Action
{
    // Public Variables
    // new readonly ActionType actionType = ActionType.GoTo;

    [SerializeField] protected Vector3 targetOffset = new Vector3();

    // Private Functions
    private void SetDestinationTo(AI AI)
    {
        // If we have a target, set a destination to the target
        if (target == null && targetTag != string.Empty)
            target = GameObject.FindWithTag(targetTag);

        if (target != null && AI.Agent != null)
        {
            // running = true;
            Vector3 targetPosition = target.transform.position;
            Vector3 adjustedTarget = targetPosition += targetOffset;
            AI.Agent.SetDestination(adjustedTarget);
        }
    }

    // Override Functions
    public override bool PreAction(AI AI)
    {
        running = true;
        SetDestinationTo(AI);
        return true;
    }

    public override bool DuringAction(AI AI)
    {
        if (AI.canUpdatePath)
            SetDestinationTo(AI);
        
        return true;
    }

    public override bool PostAction(AI AI)
    {
        return true;
    }

    public override bool IsComplete(AI AI)
    {
        if (AI.Agent != null)
        {
            if (AI.Agent.hasPath && AI.Agent.remainingDistance <= AI.Agent.stoppingDistance && !AI.Agent.pathPending)
            {
                AI.Agent.ResetPath();
                return true;
            }
        }
        else
            Debug.LogWarning("ERROR: GoTo Core Action cannot find AI Agent. Cannot complete action.");

        return false;
    }


}
