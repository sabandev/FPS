using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// GOAP_Goal
/// Template for all goals; they hold a dictionary which allows the raw <string, int> to be referenced in code
/// </summary>
[System.Serializable]
public class GOAP_Goal
{
    // Public Variables
    public Dictionary<string, int> goalDictionary;

    public string goalName = "goal";
    public int importance = 1;
    public bool infinte = false;
    public bool enabled = true;

    // Constructor
    public GOAP_Goal(string n = "goal", bool inf = true, int imp = 1)
    {
        infinte = inf;
        goalName = n;
        importance = imp;

        goalDictionary = new Dictionary<string, int>();
        goalDictionary.Add(n, imp);
    }
}
