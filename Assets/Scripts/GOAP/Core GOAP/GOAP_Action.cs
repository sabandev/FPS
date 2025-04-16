using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// GOAP_Action
/// Template for all GOAP actions
/// </summary>
public abstract class GOAP_Action : MonoBehaviour
{
    // Public Variables
    public string actionName = "Action";

    public float cost = 1.0f;
    public float duration = 0.0f;

    public bool running = false;

    public GameObject target;
    public string targetTag;

    public GOAP_WorldState[] worldStatePreconditions;
    public GOAP_WorldState[] worldStateEffects;

    // public NavMeshAgent agent;

    public GOAP_WorldStates agentBeliefs;

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
    public bool IsAchievable()
    {
        // We would assume all of our actions intend to be achievable
        // but you would put code here to specify if the action is not achievable right away for any reason
        return true;
    }

    public bool IsAchievableGiven(Dictionary<string, int> conditions)
    {
        foreach (KeyValuePair<string, int> c in preconditions)
        {
            if (!conditions.ContainsKey(c.Key))
                return false;
        }

        return true;
    }

    public abstract bool PreAction();
    public virtual bool DuringAction() { return true; }
    public abstract bool PostAction();
    public abstract bool IsComplete();

    // Private Functions
    protected virtual void Awake()
    {
        // agent = gameObject.GetComponent<NavMeshAgent>();

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
