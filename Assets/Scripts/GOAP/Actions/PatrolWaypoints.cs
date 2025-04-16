using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// PatrolWaypoints
/// GoTo ACTION
/// Visits a list of waypoints in order, skipping those that are inactive and looping back to the first waypoint after visiting all of them
/// </summary>
public class PatrolWaypoints : GOAP_ACTION_GoTo
{
    // Inspector Variables
    [SerializeField] private List<Transform> waypoints;

    // Private Variables
    private int _currentWaypointIndex = 0;

    // Overriden functions
    public override bool PreAction()
    {
        if (NextAvailableWaypoint() == null)
            return false;
        else
        {
            target = NextAvailableWaypoint();
            return base.PreAction();
        }
    }

    public override bool DuringAction()
    {
        if (!target.activeSelf)
            target = NextAvailableWaypoint();

        return base.DuringAction();
    }

    public override bool PostAction()
    {
        if (_currentWaypointIndex < waypoints.Count - 1)
            _currentWaypointIndex++;
        else
            _currentWaypointIndex = 0;

        return true;
    }

    private GameObject NextAvailableWaypoint()
    {
        if (waypoints.Count == 0)
        {
            Debug.LogWarning("WARNING: No waypoints have been set in this action. Must have waypoints to continue");
            return null;
        }

        bool activeWaypointFound = false;

        for (int i = 0; i < waypoints.Count; i++)
        {
            if (waypoints[i].gameObject.activeSelf)
                activeWaypointFound = true;
        }

        if (activeWaypointFound)
        {
            for (int w = _currentWaypointIndex; w < waypoints.Count; w++)
            {
                if (waypoints[w].gameObject.activeSelf)
                    return waypoints[w].gameObject;
                
                if (w == waypoints.Count - 1)
                    _currentWaypointIndex = 0;
            }
        }

        return null;
    }
}