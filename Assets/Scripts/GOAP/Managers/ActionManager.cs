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
    public List<GOAP_Action> GetActions()
    {
        GOAP_Action[] rawActions = GetComponents<GOAP_Action>();
        List<GOAP_Action> actions = new List<GOAP_Action>();
        
        if (rawActions.Length == 0)
        {
            Debug.LogWarning("WARNING: No actions on ActionManager GameObject");
            return null;
        }

        foreach (GOAP_Action a in rawActions)
            actions.Add(a);

        return actions;
    }
}
