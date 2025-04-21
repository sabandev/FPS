using UnityEngine;

/// <summary>
/// MoveToTarget
/// GoTo ACTION
/// Involves moving the AI to a specified target
/// </summary>
[CreateAssetMenu(menuName = "GOAP/Actions/GoTo/Move To Target")]
public class MoveToTarget : GoTo
{
    // Override Functions
    public override bool PreAction(AI AI)
    {
        target = AI.target;
        return base.PreAction(AI);
    }

    public override bool PostAction(AI AI)
    {
        return base.PostAction(AI);
    }
}
