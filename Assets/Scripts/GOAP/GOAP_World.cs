using UnityEngine;

/// <summary>
/// GOAP_World
/// Contains all information about the game world for the AI
/// Singleton to ensure only one instane of GOAP_World
/// Sealed to avoid conflicts with multiple requests and ensure no inheritance
/// </summary>
public sealed class GOAP_World
{
    // Private variables
    private static readonly GOAP_World instance = new GOAP_World();
    
    private static GOAP_WorldStates world;

    private GOAP_World() {}

    // Constructor
    static GOAP_World()
    {
        world = new GOAP_WorldStates();
    }

    public static GOAP_World Instance
    {
        get {  return instance; }
    }

    // Public Functions

    public GOAP_WorldStates GetWorld()
    {
        return world;
    }
}
