using UnityEngine;

/// <summary>
/// MoveToTarget
/// GoTo ACTION
/// Involves moving the AI to a specified target
/// </summary>
[CreateAssetMenu(menuName = "GOAP/Actions/GoTo/Move To Target")]
public class MoveToTarget : GoTo
{
    public override bool PreAction(GOAP_Agent AI)
    {
        return base.PreAction(AI);
    }

    public override bool PostAction(GOAP_Agent AI)
    {
        return base.PostAction(AI);
    }
}
