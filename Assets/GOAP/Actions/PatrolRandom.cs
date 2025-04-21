using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// PatrolRandom
/// GoTo ACTION
/// Finds a number of random points on the NavMesh, cycles through each point
/// </summary>
[CreateAssetMenu(menuName = "GOAP/Actions/GoTo/PatrolRandom")]
public class PatrolRandom : GoTo
{
    // Inspector Variables
    [SerializeField] private int numberOfRandomWaypoints = 10;
    [SerializeField] private float waypointRange = 75.0f;

    // Private Variables
    private List<Vector3> _randomWaypointPositions = new List<Vector3>();

    private int _currentWaypointIndex = 0;

    private bool _visitedEveryWaypoint = false;

    // Private functions
    private void GetRandomPoints(Vector3 center, float range)
    {
        while (_randomWaypointPositions.Count < numberOfRandomWaypoints)
        {
            Vector3 randomPoint = center + Random.insideUnitSphere * range;
            Vector3 result = Vector3.zero;

            NavMeshHit hit;

            if (NavMesh.SamplePosition(randomPoint, out hit, 1f, NavMesh.AllAreas))
            {
                result = hit.position;
                _randomWaypointPositions.Add(result);
            }
        }
    }

    // Overriden functions
    public override bool PreAction(AI AI)
    {
        GetRandomPoints(AI.transform.position, waypointRange);

        // Create a gameObject at the random position to set as the target
        target = Instantiate(new GameObject("$$Random Waypoint$$"), _randomWaypointPositions[_currentWaypointIndex], Quaternion.identity);

        return base.PreAction(AI);
    }

    public override bool DuringAction(AI AI)
    {
        if (AI.agent.remainingDistance < AI.stoppingDistance)
        {
            // Destroy the target and any other clone instances
            Destroy(target);
            Destroy(GameObject.Find("$$Random Waypoint$$"));

            // Increment to the next waypoint
            if (_currentWaypointIndex != _randomWaypointPositions.Count - 1)
            {
                _currentWaypointIndex++;

                // Create a gameObject at the random position to set as the target
                target = Instantiate(new GameObject("$$Random Waypoint$$"), _randomWaypointPositions[_currentWaypointIndex], Quaternion.identity);
            }
            else
                _visitedEveryWaypoint = true;
        }

        return base.DuringAction(AI);
    }

    public override bool PostAction(AI AI)
    {
        _currentWaypointIndex = 0;
        _visitedEveryWaypoint = false;

        // Destroy the target and any other clone instances
        Destroy(target);
        Destroy(GameObject.Find("$$Random Waypoint$$"));

        return true;
    }

    public override bool IsComplete(AI AI)
    {
        return _visitedEveryWaypoint;
    }
}