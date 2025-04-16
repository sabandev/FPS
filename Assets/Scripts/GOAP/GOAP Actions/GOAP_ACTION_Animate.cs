using UnityEngine;

/// <summary>
/// GOAP_ACTION_Animate
/// Inherits GOAP_Action
/// Is the template for all actions that involve animating in one place
/// </summary>
public class GOAP_ACTION_Animate : GOAP_Action
{
    // Private Variables
    protected bool _hasAnimated = false;

    // Override Functions
    public override bool PreAction()
    {
        running = true;
        _hasAnimated = false;
        return true;
    }

    public override bool PostAction()
    {
        return true;
    }

    public override bool IsComplete()
    {
        return _hasAnimated;
    }
}
