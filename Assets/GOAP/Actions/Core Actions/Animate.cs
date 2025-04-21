using UnityEngine;

/// <summary>
/// GOAP_ACTION_Animate
/// Inherits GOAP_Action
/// Is the template for all actions that involve animating in one place
/// </summary>
public class Animate : GOAP_Action
{
    // Public Variables
    // new readonly ActionType actionType = ActionType.Animate;

    // Private Variables
    protected bool _hasAnimated = false;

    // Override Functions
    public override bool PreAction(AI AI)
    {
        running = true;
        _hasAnimated = false;
        return true;
    }

    public override bool PostAction(AI AI)
    {
        return true;
    }

    public override bool IsComplete(AI AI)
    {
        return _hasAnimated;
    }
}
