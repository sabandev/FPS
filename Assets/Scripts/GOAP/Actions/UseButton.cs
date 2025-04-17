using UnityEngine;

/// <summary>
/// UseButton
/// Animate ACTION
/// Involves playing an animation that uses a button
/// </summary>
[CreateAssetMenu(menuName = "GOAP/Actions/Animate/Use button")]
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
