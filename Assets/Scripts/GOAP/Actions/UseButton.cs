using UnityEngine;

/// <summary>
/// UseButton
/// Animate ACTION
/// Involves playing an animation that uses a button
/// </summary>
public class UseButton : Animate
{
    // Override Functions
    public override bool PreAction(GOAP_Agent AI)
    {
        base.PreAction(AI);

        _hasAnimated = true;

        return true;
    }
}
