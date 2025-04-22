using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// GOAP_GoalSet.
/// A scriptable object that holds a list of GOAP_Goals.
/// Applied to AI agents to assing goals.
/// </summary>
[CreateAssetMenu(menuName = "GOAP/Goals/Goal Set")]
public class GOAP_GoalSet : ScriptableObject
{
    public List<GOAP_Goal> goals;
}