using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// PatrolWaypoints
/// GoTo ACTION
/// Visits a list of waypoints in order, skipping those that are inactive and looping back to the first waypoint after visiting all of them
/// </summary>
[CreateAssetMenu(menuName = "GOAP/Actions/GoTo/PatrolWaypoints")]
public class PatrolWaypoints : GoTo
{
    // Overriden functions
    public override bool PreAction(GOAP_Agent AI)
    {
        if (NextAvailableWaypoint(AI) == null)
            return false;
        else
        {
            target = NextAvailableWaypoint(AI);
            return base.PreAction(AI);
        }
    }

    public override bool DuringAction(GOAP_Agent AI)
    {
        if (!target.activeSelf)
            target = NextAvailableWaypoint(AI);

        return base.DuringAction(AI);
    }

    public override bool PostAction(GOAP_Agent AI)
    {
        if (AI.currentWaypointIndex < AI.waypoints.Count - 1)
            AI.currentWaypointIndex++;
        else
            AI.currentWaypointIndex = 0;

        return true;
    }

    private GameObject NextAvailableWaypoint(GOAP_Agent AI)
    {
        if (AI.waypoints.Count == 0)
        {
            Debug.LogWarning("WARNING: No waypoints have been set in this action. Must have waypoints to continue");
            return null;
        }

        bool activeWaypointFound = false;

        for (int i = 0; i < AI.waypoints.Count; i++)
        {
            if (AI.waypoints[i].gameObject.activeSelf)
                activeWaypointFound = true;
        }

        if (activeWaypointFound)
        {
            for (int w = AI.currentWaypointIndex; w < AI.waypoints.Count; w++)
            {
                if (AI.waypoints[w].gameObject.activeSelf)
                    return AI.waypoints[w].gameObject;
                
                if (w == AI.waypoints.Count - 1)
                    AI.currentWaypointIndex = 0;
            }
        }

        return null;
    }
}