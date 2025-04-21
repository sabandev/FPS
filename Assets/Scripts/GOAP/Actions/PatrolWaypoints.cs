using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// PatrolWaypoints
/// GoTo ACTION
/// Visits a list of waypoints in order, skipping those that are inactive and looping back to the first waypoint after visiting all of them
/// </summary>
[CreateAssetMenu(menuName = "GOAP/Actions/GoTo/PatrolWaypoints")]
public class PatrolWaypoints : GoTo
{
    // Private Variables
    private List<Transform> activeWaypoints = new List<Transform>();

    // Overriden functions
    public override bool PreAction(AI AI)
    {
        // Add active waypoints from the agent's waypoints list to our list
        if (AI.waypoints.Count == 0)
        {
            Debug.LogWarning("WARNING: No waypoints assigned to AI. Cannot patrol null waypoints.");
            return base.PreAction(AI);
        }

        for (int i = 0; i < AI.waypoints.Count; i++)
        {
            if (AI.waypoints[i].gameObject.activeSelf)
                activeWaypoints.Add(AI.waypoints[i]);
        }

        target = NextAvailableWaypoint(AI);
        return base.PreAction(AI);
}

    public override bool DuringAction(AI AI)
    {
        if (!target.activeSelf)
            target = NextAvailableWaypoint(AI);

        if (AI.agent != null && AI.agent.hasPath && AI.agent.remainingDistance < AI.stoppingDistance)
        {
            AI.currentWaypointIndex++;
            target = NextAvailableWaypoint(AI);
        }

        return base.DuringAction(AI);
    }

    public override bool PostAction(AI AI)
    {
        AI.currentWaypointIndex = 0;
        activeWaypoints = new List<Transform>();
        return true;
    }

    public override bool IsComplete(AI AI)
    {
        if (AI.currentWaypointIndex == activeWaypoints.Count)
            return true;

        return false;
    }

    // Private Functions
    private GameObject NextAvailableWaypoint(AI AI)
    {
        // Return next waypoint as the target
        for (int w = AI.currentWaypointIndex; w < activeWaypoints.Count; w++)
        {
            if (activeWaypoints[w].gameObject.activeSelf)
                return activeWaypoints[w].gameObject;
        }

        return null;
    }
}