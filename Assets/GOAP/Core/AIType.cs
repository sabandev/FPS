using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// AIType.
/// A scriptable object that specifies a list of GOAP_Actions.
/// Assigned to an AI agent to assign them available actions they can use.
/// </summary>
[CreateAssetMenu(menuName = "GOAP/AI/AI Type")]
public class AIType : ScriptableObject
{
    public string typeName;

    public List<GOAP_Action> availableActions;
}
