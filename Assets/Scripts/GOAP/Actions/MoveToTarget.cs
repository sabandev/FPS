using System;
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
        if (AI.target != null)
            target = AI.target;
        else
            Debug.LogWarning("WARNING: AI has no target gameObject. Cannot move to null target.");
        return base.PreAction(AI);
    }

    public override bool PostAction(AI AI)
    {
        return base.PostAction(AI);
    }
}
