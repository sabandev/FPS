using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ActionManager.
/// Manager.
/// References GOAP Actions and returns a list of GOAP_Actions so that AI know what actions they are allowed to use.
/// </summary>
public class ActionManager : MonoBehaviour
{
    public List<GOAP_Action> unspecifiedAIActions;
    public List<GOAP_Action> pathfinderAIActions;
    public List<GOAP_Action> idlerAIActions;

    // Public Functions

    public List<GOAP_Action> GetActions(AI AI)
    {
        // Return the appropriate list of actions depending on the AI's type

        if (AI.selectedAIType == AIType. && pathfinderAIActions != null)
            return pathfinderAIActions;
        else if (AI.aiType == AIType.Idler && idlerAIActions != null)
            return idlerAIActions;

        return unspecifiedAIActions;
    }
}
