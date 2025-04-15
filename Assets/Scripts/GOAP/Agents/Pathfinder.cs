using System.Security.Cryptography;
using UnityEngine;

public class Pathfinder : GOAP_Agent
{
    // Functions
    new void Start()
    {
        base.Start();

        GOAP_SubGoal g1 = new GOAP_SubGoal("isWaiting", 1, true);
        goals.Add(g1, 3);
    }
}
