using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// GOAP_ACTION_GoTo
/// Inherits GOAP_Action
/// Is the template for all actions that involve moving the AI from point A to point B
/// </summary>
public class GoTo : GOAP_Action
{
    // Private Functions
    private void MoveToTarget(GOAP_Agent AI)
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
    public override bool PreAction(GOAP_Agent AI)
    {
        // Debug.Log("Called GoTo PreAction");
        MoveToTarget(AI);
        return true;
    }

    public override bool DuringAction(GOAP_Agent AI)
    {
        MoveToTarget(AI);
        return true;
    }

    public override bool PostAction(GOAP_Agent AI)
    {
        return true;
    }

    public override bool IsComplete(GOAP_Agent AI)
    {
        if (AI.agent != null)
        {
            if (AI.agent.hasPath && AI.agent.remainingDistance < AI.agent.stoppingDistance)
                return true;
        }
        else
        {
            if (Vector3.Distance(AI.transform.position, target.transform.position) < 3.0f)
                return true;
        }

        return false;
    }


}
