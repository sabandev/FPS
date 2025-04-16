using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// PatrolRandom
/// GoTo ACTION
/// Finds 50 random points on the NavMesh, cycles through each point
/// </summary>
public class PatrolRandom : GOAP_ACTION_GoTo
{
    // Private Variables
    private List<Vector3> randomWaypointPositions = new List<Vector3>();

    private int _currentWaypointIndex = 0;

    // Private functions
    private void RandomPoint(Vector3 center, float range)
    {
        while (randomWaypointPositions.Count < 50)
        {
            Vector3 randomPoint = center + Random.insideUnitSphere * range;
            Vector3 result = Vector3.zero;

            NavMeshHit hit;

            if (NavMesh.SamplePosition(randomPoint, out hit, 1f, NavMesh.AllAreas))
            {
                result = hit.position;
                randomWaypointPositions.Add(result);
            }
        }
    }

    private void Start()
    {
        RandomPoint(transform.position, 50.0f);
    }

    // Overriden functions
    public override bool PreAction()
    {
        if (_currentWaypointIndex == randomWaypointPositions.Count)
            _currentWaypointIndex = 0;

        // Create a gameObject at the random position to set as the target
        target = Instantiate(new GameObject("$$Random Waypoint$$"), randomWaypointPositions[_currentWaypointIndex], Quaternion.identity);

        // Debug.Log(_currentWaypointIndex);

        return base.PreAction();
    }

    public override bool PostAction()
    {
        // Destroy the target and any other clone instances
        Destroy(target);
        Destroy(GameObject.Find("$$Random Waypoint$$"));
        _currentWaypointIndex++;

        return true;
    }
}