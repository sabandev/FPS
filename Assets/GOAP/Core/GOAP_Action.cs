using System.Collections.Generic;
using UnityEngine;

public enum ActionType
{
    GoTo,
    Animate,
    UseSmartObject
}

/// <summary>
/// GOAP_Action
/// Template for all GOAP actions
/// </summary>
public abstract class GOAP_Action : ScriptableObject
{
    // Public Variables
    public ActionType actionType { get; private set; }

    public string actionName = "Generic Action";

    public float cost = 1.0f;
    public float duration = 0.0f;

    public bool running = false;

    public AI agent;

    // NOTE: move specific variables to their dedicated GOAP Actions
    public GameObject target;
    public string targetTag;

    public GOAP_WorldState[] worldStatePreconditions;
    public GOAP_WorldState[] worldStateEffects;

    // public NavMeshAgent agent;

    // public GOAP_WorldStates agentBeliefs;

    // Used to translate the WorldState[] into dictionaries
    public Dictionary<string, int> preconditions;
    public Dictionary<string, int> effects;

    // Constructor
    public GOAP_Action()
    {
        preconditions = new Dictionary<string, int>();
        effects = new Dictionary<string, int>();
    }

    // Public Functions
    public virtual bool IsAchievable()
    {
        // We would assume all of our actions intend to be achievable
        // but you would put code here to specify if the action is not achievable right away for any reason
        return true;
    }

    public virtual bool IsAchievableGiven(Dictionary<string, int> conditions)
    {
        foreach (KeyValuePair<string, int> c in preconditions)
        {
            if (!conditions.ContainsKey(c.Key))
                return false;
        }

        return true;
    }

    public abstract bool PreAction(AI AI);
    public virtual bool DuringAction(AI AI) { return true; }
    public abstract bool PostAction(AI AI);
    public abstract bool IsComplete(AI AI);

    // Private Functions
    protected virtual void Awake()
    {
        if (worldStatePreconditions != null)
        {
            foreach (GOAP_WorldState s in worldStatePreconditions)
            {
                preconditions.Add(s.key, s.value);
            }
        }

        if (worldStateEffects != null)
        {
            foreach (GOAP_WorldState s in worldStateEffects)
            {
                effects.Add(s.key, s.value);

            }
        }
    }
}
