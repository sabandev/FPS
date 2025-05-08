using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// SoundType.
/// Enumeration.
/// Stores all of the different types of sound a sound can be.
/// </summary>
public enum SoundType
{
    Player,
    AI,
    Environment
}

/// <summary>
/// SoundEvent.
/// A class that stores information about a sound, including its position, radius and timestamp.
/// </summary>
public class SoundEvent
{
    public SoundType type;
    public Vector3 position;
    public float volume;
    public GameObject source;

    public SoundEvent(SoundType t, Vector3 pos, float vol, GameObject src)
    {
        type = t;
        position = pos;
        volume = vol;
        source = src;
    }
}

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

    // Private Variables
    public static List<SoundEvent> ActiveSounds = new List<SoundEvent>();

    public bool noSounds = false;

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

    private void LateUpdate()
    {
        // Clear and update the world states
        worldStates.Clear();
        foreach (var pair in worldStatesClass.states)
            worldStates.Add(new GOAP_WorldState { key = pair.Key, value = pair.Value });
    }

    public static void EmitSound(SoundType type, Vector3 position, float volume, GameObject source)
    {
        ActiveSounds.Add(new SoundEvent(type, position, volume, source));
    }

    public static void ClearActiveSounds()
    {
        ActiveSounds.Clear();
    }

    // public List<SoundEvent> GetRecentSounds(float maxAge = 1.0f)
    // {
    //     List<SoundEvent> recentSoundsList = activeSounds;

    //     recentSoundsList.RemoveAll(s => Time.time - s.timestamp > maxAge);

    //     if (recentSoundsList == null)
    //         noSounds = true;

    //     return recentSoundsList;
    // }

    // Public Functions
}
