using UnityEngine;

/// <summary>
/// UseButton
/// Animate ACTION
/// Involves playing an animation that uses a button
/// </summary>
[CreateAssetMenu(menuName = "GOAP/Actions/UseSmartObject/Use button")]
public class UseButton : UseSmartObject
{
    // Override Functions
    public override bool PreAction(AI AI)
    {
        base.PreAction(AI);

        _hasUsedSmartObject = true;

        return true;
    }
}
