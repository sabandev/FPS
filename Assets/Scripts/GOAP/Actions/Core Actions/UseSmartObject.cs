using UnityEngine;

/// <summary>
/// UseSmartObject
/// Inherits GOAP_Action
/// 
/// </summary>
public class UseSmartObject : GOAP_Action
{
    // Private Variables
    protected bool _hasUsedSmartObject = false;

    public override bool PreAction(GOAP_Agent AI)
    {
        running = true;
        _hasUsedSmartObject = false;
        return true;
    }

    public override bool PostAction(GOAP_Agent AI)
    {
        return true;
    }

    public override bool IsComplete(GOAP_Agent AI)
    {
        return _hasUsedSmartObject;
    }

}
