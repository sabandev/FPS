using Unity.VisualScripting;
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

    // Private Functions
    private void MoveToTarget(AI AI)
    {
        // If we have a target, set a destination to the target
        if (target == null && targetTag != string.Empty)
            target = GameObject.FindWithTag(targetTag);

        if (target != null && AI.agent != null)
        {
            running = true;
            AI.agent.SetDestination(target.transform.position);
        }
    }

    // Override Functions
    public override bool PreAction(AI AI)
    {
        MoveToTarget(AI);
        return true;
    }

    public override bool DuringAction(AI AI)
    {
        MoveToTarget(AI);
        return true;
    }

    public override bool PostAction(AI AI)
    {
        return true;
    }

    public override bool IsComplete(AI AI)
    {
        if (AI.agent != null)
        {
            if (AI.agent.hasPath && AI.agent.remainingDistance < AI.agent.stoppingDistance)
                return true;
        }
        else
            Debug.LogWarning("ERROR: GoTo Core Action cannot find AI Agent. Cannot complete action.");

        return false;
    }


}
