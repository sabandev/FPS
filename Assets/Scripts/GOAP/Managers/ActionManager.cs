using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// ActionManager
/// Manager
/// References GOAP Actions to allow AI to be assigned their allowed actions
/// </summary>
public class ActionManager : MonoBehaviour
{
    public List<GOAP_Action> unspecifiedAIActions;
    public List<GOAP_Action> pathfinderAIActions;
    public List<GOAP_Action> idlerAIActions;

    // Private Functions

    // Public Functions
    public List<GOAP_Action> GetActions(AI AI)
    {
        // Convert the 
        // List<GOAP_Action> allActions = GetComponents<GOAP_Action>().ToList<GOAP_Action>();

        if (AI.aiType == AIType.Pathfinder && pathfinderAIActions != null)
            return pathfinderAIActions;
        else if (AI.aiType == AIType.Idler && idlerAIActions != null)
            return idlerAIActions;

        return unspecifiedAIActions;
    }
}
