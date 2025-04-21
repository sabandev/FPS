using UnityEngine;

/// <summary>
/// UseSmartObject
/// Inherits GOAP_Action
/// 
/// </summary>
public class UseSmartObject : GOAP_Action
{
    // Public Variables
    // new readonly ActionType actionType = ActionType.UseSmartObject;

    // Private Variables
    protected bool _hasUsedSmartObject = false;

    public override bool PreAction(AI AI)
    {
        running = true;
        _hasUsedSmartObject = false;
        return true;
    }

    public override bool PostAction(AI AI)
    {
        return true;
    }

    public override bool IsComplete(AI AI)
    {
        return _hasUsedSmartObject;
    }

}
