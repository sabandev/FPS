using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// GOAP_World
/// Contains all information about the game world for the AI.
/// Singleton to ensure only one instance of GOAP_World.
/// Sealed to avoid conflicts with multiple requests and ensure no inheritance.
/// </summary>
public sealed class GOAP_World : MonoBehaviour
{
    // Public Variables
    public static GOAP_World Instance { get; private set; }

    public GOAP_WorldStates worldStatesClass = new GOAP_WorldStates();
    public List<GOAP_Goal> goals = new List<GOAP_Goal>();

    // Inspector Variables
    public List<GOAP_WorldState> worldStates = new List<GOAP_WorldState>();

    // Private Functions
    private void Awake()
    {
        // Preserve singleton
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // Set any inspector-assigned states to their correct values
        foreach (var ws in worldStates)
            worldStatesClass.states[ws.key] = ws.value;
    }

    private void Update()
    {
        worldStates.Clear();
        foreach (var pair in worldStatesClass.states)
            worldStates.Add(new GOAP_WorldState { key = pair.Key, value = pair.Value });
    }

    // Public Functions
}
