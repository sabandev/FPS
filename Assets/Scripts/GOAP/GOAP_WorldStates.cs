using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

/// <summary>
/// GOAP_WorldState
/// A template for world states
/// Serializable so it can be seen in the Unity Inspector
/// </summary>
[System.Serializable]
public class GOAP_WorldState
{
    // Public Variables
    public string key;
    
    public int value;
}

/// <summary>
/// GOAP_WorldStates
/// The collection of world states
/// Contains helper functions for world states
/// </summary>
public class GOAP_WorldStates
{
    // Public Variables
    public Dictionary<string, int> states;

    // Constructor
    public GOAP_WorldStates()
    {
        states = new Dictionary<string, int>();
    }

    // Public Functions
    public Dictionary<string, int> GetStates()
    {
        return states;
    }

    public bool HasState(string key)
    {
        return states.ContainsKey(key);
    }

    public void AddState(string key, int value)
    {
        states.Add(key, value);
    }

    public void ModifyState(string key, int value)
    {
        if (states.ContainsKey(key))
        {
            states[key] += value;

            // If the key is negative, remove the state
            if (states[key] <= 0)
                RemoveState(key);
            else
                states.Add(key, value);
        }
    }

    public void SetState(string key, int value)
    {
        if (states.ContainsKey(key))
            states[key] = value;
        else
            states.Add(key, value);
    }

    public void RemoveState(string key)
    {
        if (states.ContainsKey(key))
            states.Remove(key);
    }

    // Private Functions
}