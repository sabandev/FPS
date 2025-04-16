using UnityEngine;

/// <summary>
/// UseButton
/// Animate ACTION
/// Involves playing an animation that uses a button
/// </summary>
public class UseButton : GOAP_ACTION_Animate
{
    // Override Functions
    public override bool PreAction()
    {
        base.PreAction();

        _hasAnimated = true;

        return true;
    }
}
