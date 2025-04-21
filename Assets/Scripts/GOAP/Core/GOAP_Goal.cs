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

    public string name = "goal";
    public bool removeAfterCompletion = true;
    public int importance = 1;

    // Constructor
    public GOAP_Goal(string s = "goal", bool r = true, int i = 1)
    {
        removeAfterCompletion = r;
        name = s;
        importance = i;

        goalDictionary = new Dictionary<string, int>();
        goalDictionary.Add(s, i);
    }
}
