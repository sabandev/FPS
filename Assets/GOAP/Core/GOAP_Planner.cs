using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using System.Collections;
using Unity.VisualScripting;

/// <summary>
/// Node
/// Represents each node in the completed plan graph
/// </summary>
public class Node
{
    // Public Variables
    public Node parent;
    
    public float cost;

    public Dictionary<string, int> state;

    public GOAP_Action action;

    // Constructor
    public Node (Node parent, float cost, Dictionary<string, int> states, GOAP_Action action)
    {
        this.parent = parent;
        this.cost = cost;
        this.state = new Dictionary<string, int>(states); // We want a copy of all the states, not to reference it outright
        this.action = action;
    }
}

/// <summary>
/// GOAP_Planner
/// Creates a graph representing the complete plan of actions to achieve a goal
/// </summary>
public class GOAP_Planner
{
    // Public Functions
    public Queue<GOAP_Action> Plan(List<GOAP_Action> actions, Dictionary<string, int> goal, GOAP_WorldStates states, bool debug)
    {
        // Funnel out actions that are not achievable
        List<GOAP_Action> achievableActions = new List<GOAP_Action>();
        foreach (GOAP_Action a in actions)
        {
            if (a.IsAchievable())
                achievableActions.Add(a);
        }

        // Create starting node
        List<Node> leaves = new List<Node>();
        Node start = new Node(null, 0.0f, GOAP_World.Instance.worldStatesClass.GetStates(), null);

        // Figure out if we have found a plan
        bool successfulGraph = BuildGraph(start, leaves, achievableActions, goal);
        if (!successfulGraph)
        {
            if (debug)
                Debug.Log($"PLANNER: No plan found for goal: {goal.First().Key}");

            return null;
        }

        // Find cheapest node
        Node cheapest = null;
        foreach (Node leaf in leaves)
        {
            if (cheapest == null)
                cheapest = leaf;
            else if (leaf.cost < cheapest.cost)
                cheapest = leaf;
        }

        // Create a resulting plan
        List<GOAP_Action> resultPlan = new List<GOAP_Action>();
        Node n = cheapest;
        while (n != null)
        {
            if (n.action != null)
            {
                resultPlan.Insert(0, n.action);
            }
            n = n.parent;
        }

        // Create a queue of actions the AI can perform
        Queue<GOAP_Action> queue = new Queue<GOAP_Action>();
        foreach (GOAP_Action a in resultPlan)
        {
            queue.Enqueue(a);
        }

        // Print the plan with some formatting
        string planToString = "";
        int counter = 0;
        foreach (GOAP_Action a in queue)
        {
            counter += 1;

            if (counter == queue.Count)
                planToString += a.actionName; 
            else
                planToString += a.actionName + " -> ";
        }
        Debug.Log("Plan: " + planToString);

        return queue;
    }

    // Private Functions
    private bool BuildGraph(Node parent, List<Node> leaves, List<GOAP_Action> achievableActions, Dictionary<string, int> goal)
    {
        bool pathFound = false;

        // Check if achievable actions are achievable given the world state
        foreach (GOAP_Action action in achievableActions)
        {
            if (action.IsAchievableGiven(parent.state))
            {
                Dictionary<string, int> currentState = new Dictionary<string, int>(parent.state);

                foreach (KeyValuePair<string, int> effect in action.effects)
                {
                    if (!currentState.ContainsKey(effect.Key))
                        currentState.Add(effect.Key, effect.Value); 
                }

                Node node = new Node(parent, parent.cost + action.cost, currentState, action);

                // Check if a path has been found
                if (GoalAchieved(goal, currentState))
                {
                    leaves.Add(node);
                    pathFound = true;
                }
                else
                {
                    // Ensure achievable actions is reduced the further along the graph you go
                    List<GOAP_Action> filteredActions = FilteredActions(achievableActions, action);
                    bool found = BuildGraph(node, leaves, filteredActions, goal);

                    if (found)
                        pathFound = true;
                }
            }
        }

        return pathFound;
    }

    private bool GoalAchieved(Dictionary<string, int> goal, Dictionary<string, int> state)
    {
        // Check all conditions of the goal are met
        foreach (KeyValuePair<string, int> g in goal)
        {
            if (!state.ContainsKey(g.Key))
                return false;
        }
        return true;
    }

    private List<GOAP_Action> FilteredActions(List<GOAP_Action> actions, GOAP_Action actionToRemove)
    {
        List<GOAP_Action> filtered = new List<GOAP_Action>();

        foreach (GOAP_Action a in actions)
        {
            if (!a.Equals(actionToRemove))
                filtered.Add(a);
        }
        return filtered;
    }
}
