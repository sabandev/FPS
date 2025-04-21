using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary>
/// AIEditor.
/// Editor Script.
/// Creates a custom inspector for the AI class.
/// </summary>
[CustomEditor(typeof(AI))]
public class AIEditor : Editor
{
    #region Serialized Properties
    SerializedProperty aiType;
    SerializedProperty walkingSpeed;
    SerializedProperty runningSpeed;
    SerializedProperty rotationSpeed;
    SerializedProperty stoppingDistance;
    SerializedProperty availableActions;
    SerializedProperty goals;
    SerializedProperty currentAction;
    SerializedProperty targetGO;
    SerializedProperty waypoints;
    SerializedProperty currentWaypointIndex;
    SerializedProperty jumpHeight;
    SerializedProperty jumpDuration;
    SerializedProperty ladderClimbDuration;
    SerializedProperty assignTargetGO;
    SerializedProperty assignWaypoints;
    SerializedProperty selectedAITypeIndex;
    SerializedProperty agentManager;
    #endregion

    #region Private Properties
    private AgentManager _agentManager;

    private GUIStyle _header1Style;
    private GUIStyle _header2Style;
    private GUIStyle _titleStyle;
    #endregion

    private void InitialiseCustomStyles()
    {
        #region Header 1
        _header1Style = new GUIStyle(EditorStyles.boldLabel);
        _header1Style.fontSize = 16;
        _header1Style.normal.textColor = Color.white;
        _header1Style.margin = new RectOffset(0, 0, 0, 10);
        _header1Style.alignment = TextAnchor.MiddleLeft;
        #endregion

        #region Header 2
        _header2Style = new GUIStyle(EditorStyles.boldLabel);
        _header2Style.fontSize = 12;
        _header2Style.normal.textColor = Color.white;
        _header2Style.alignment = TextAnchor.MiddleLeft;
        #endregion

        #region Title
        _titleStyle = new GUIStyle(EditorStyles.boldLabel);
        _titleStyle.fontSize = 20;
        _titleStyle.normal.textColor = Color.white;
        _titleStyle.alignment = TextAnchor.MiddleCenter;
        #endregion
    }

    private void OnEnable()
    {
        #region Set Serialized Properties
        aiType = serializedObject.FindProperty("aiType");
        walkingSpeed = serializedObject.FindProperty("walkingSpeed");
        runningSpeed = serializedObject.FindProperty("runningSpeed");
        rotationSpeed = serializedObject.FindProperty("rotationSpeed");
        stoppingDistance = serializedObject.FindProperty("stoppingDistance");
        availableActions = serializedObject.FindProperty("availableActions");
        goals = serializedObject.FindProperty("goals");
        currentAction = serializedObject.FindProperty("currentAction");
        targetGO = serializedObject.FindProperty("target");
        waypoints = serializedObject.FindProperty("waypoints");
        currentWaypointIndex = serializedObject.FindProperty("currentWaypointIndex");
        jumpHeight = serializedObject.FindProperty("jumpHeight");
        jumpDuration = serializedObject.FindProperty("jumpDuration");
        ladderClimbDuration = serializedObject.FindProperty("ladderClimbDuration");
        assignTargetGO = serializedObject.FindProperty("assignTargetGameObject");
        assignWaypoints = serializedObject.FindProperty("assignWaypoints");
        selectedAITypeIndex = serializedObject.FindProperty("selectedAITypeIndex");
        agentManager = serializedObject.FindProperty("agentManager");
        #endregion

        _agentManager = FindAnyObjectByType<AgentManager>().GetComponent<AgentManager>();
    }

    public override void OnInspectorGUI()
    {

        InitialiseCustomStyles();
        serializedObject.Update();

        #region Title
        Rect rect = EditorGUILayout.GetControlRect(false, 30);
        float lineWidth = 250;
        float lineHeight = 5;

        // Left line
        EditorGUI.DrawRect(
            new Rect(rect.x, rect.y + rect.height / 2, lineWidth, lineHeight),
            Color.gray
        );

        // Right line
        EditorGUI.DrawRect(
            new Rect(rect.xMax - lineWidth, rect.y + rect.height / 2, lineWidth, lineHeight),
            Color.gray
        );

        EditorGUI.LabelField(rect, " AI ", _titleStyle);
        #endregion

        EditorGUILayout.Space(10.0f);

        #region Type
        AgentManager _agentManager = (AgentManager)agentManager.objectReferenceValue;

        if (_agentManager != null && _agentManager.aiTypes.Count > 0)
        {
            string[] aiTypeNames = _agentManager.aiTypes.ConvertAll(t => t.typeName).ToArray();
            selectedAITypeIndex.intValue = EditorGUILayout.Popup("AI Type", selectedAITypeIndex.intValue, aiTypeNames);
        }
        else
        {
            EditorGUILayout.HelpBox("Must assign an AI Type.", MessageType.Warning);
        }
        #endregion

        AI _ai = (AI)target;

        EditorGUILayout.Space(10.0f);

        #region Navigation
        GUILayout.Label("Navigation", _header1Style);

        GUILayout.Label("Movement", _header2Style);
        EditorGUILayout.PropertyField(walkingSpeed);
        EditorGUILayout.PropertyField(runningSpeed);
        EditorGUILayout.PropertyField(jumpHeight);
        EditorGUILayout.PropertyField(jumpDuration);
        EditorGUILayout.PropertyField(ladderClimbDuration);

        EditorGUILayout.Space(5.0f);

        GUILayout.Label("NavMeshAgent", _header2Style);
        EditorGUILayout.PropertyField(stoppingDistance);
        EditorGUILayout.PropertyField(rotationSpeed);

        EditorGUILayout.Space(5.0f);

        EditorGUILayout.PropertyField(assignTargetGO, new GUIContent("Target Destination"));
        if (_ai.assignTargetGameObject)
        {
            EditorGUILayout.PropertyField(targetGO, new GUIContent(""));
        }
        else
            _ai.target = null;

        EditorGUILayout.Space(5.0f);

        EditorGUILayout.PropertyField(assignWaypoints, new GUIContent("Waypoints"));
        if (_ai.assignWaypoints)
        {
            EditorGUILayout.PropertyField(waypoints, new GUIContent(""));
        }
        else
            _ai.waypoints = new List<Transform>();
        #endregion

        EditorGUILayout.Space(10.0f);

        #region Goals
        GUILayout.Label("Goals", _header1Style);

        EditorGUILayout.PropertyField(goals, new GUIContent("AI Goals"));
        #endregion

        EditorGUILayout.Space(10.0f);

        #region Debug
        GUILayout.Label("Debug", _header1Style);

        EditorGUI.BeginDisabledGroup(true);

        EditorGUILayout.PropertyField(currentAction);
        EditorGUILayout.PropertyField(availableActions, new GUIContent("Available Actions"));
        EditorGUILayout.PropertyField(currentWaypointIndex);

        EditorGUI.EndDisabledGroup();
        #endregion

        serializedObject.ApplyModifiedProperties();
    }
}
