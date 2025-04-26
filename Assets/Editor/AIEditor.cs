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
    SerializedProperty rotationTime;
    SerializedProperty angularSpeed;
    SerializedProperty stoppingDistance;
    SerializedProperty availableActions;
    SerializedProperty currentAction;
    SerializedProperty targetGO;
    SerializedProperty waypoints;
    SerializedProperty currentWaypointIndex;
    SerializedProperty jumpHeight;
    SerializedProperty jumpDuration;
    SerializedProperty ladderClimbDuration;
    SerializedProperty assignTargetGO;
    SerializedProperty assignWaypoints;
    SerializedProperty goalSet;
    SerializedProperty currentGoal;
    #endregion

    private void OnEnable()
    {
        #region Set Serialized Properties
        aiType = serializedObject.FindProperty("aiType");
        walkingSpeed = serializedObject.FindProperty("walkingSpeed");
        runningSpeed = serializedObject.FindProperty("runningSpeed");
        rotationTime = serializedObject.FindProperty("rotationTime");
        angularSpeed = serializedObject.FindProperty("angularSpeed");
        stoppingDistance = serializedObject.FindProperty("stoppingDistance");
        availableActions = serializedObject.FindProperty("availableActions");
        currentAction = serializedObject.FindProperty("currentAction");
        targetGO = serializedObject.FindProperty("target");
        waypoints = serializedObject.FindProperty("waypoints");
        currentWaypointIndex = serializedObject.FindProperty("currentWaypointIndex");
        jumpHeight = serializedObject.FindProperty("jumpHeight");
        jumpDuration = serializedObject.FindProperty("jumpDuration");
        ladderClimbDuration = serializedObject.FindProperty("ladderClimbDuration");
        assignTargetGO = serializedObject.FindProperty("assignTargetGameObject");
        assignWaypoints = serializedObject.FindProperty("assignWaypoints");
        goalSet = serializedObject.FindProperty("goalSet");
        currentGoal = serializedObject.FindProperty("currentGoal");
        #endregion
    }

    public override void OnInspectorGUI()
    {
        AI _ai = (AI)target;

        CustomEditorStyles.InitializeCustomStyles();
        serializedObject.Update();

        #region Title
        CustomEditorStyles.Title("AI");
        #endregion

        EditorGUILayout.Space(10.0f);

        #region Type
        GUILayout.Label("Type", CustomEditorStyles.header1Style);
        EditorGUILayout.PropertyField(aiType, new GUIContent("AI Type"));

        if (_ai.aiType == null)
            EditorGUILayout.HelpBox("WARNING: Must assign an AI type.", MessageType.Warning);
        #endregion


        EditorGUILayout.Space(10.0f);

        #region Navigation
        GUILayout.Label("Navigation", CustomEditorStyles.header1Style);

        GUILayout.Label("Movement", CustomEditorStyles.header2Style);
        EditorGUILayout.PropertyField(walkingSpeed);
        EditorGUILayout.PropertyField(runningSpeed);
        EditorGUILayout.PropertyField(rotationTime);
        EditorGUILayout.PropertyField(jumpHeight);
        EditorGUILayout.PropertyField(jumpDuration);
        EditorGUILayout.PropertyField(ladderClimbDuration);

        EditorGUILayout.Space(5.0f);

        GUILayout.Label("NavMeshAgent", CustomEditorStyles.header2Style);
        EditorGUILayout.PropertyField(stoppingDistance);
        EditorGUILayout.PropertyField(angularSpeed);

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
        GUILayout.Label("Goals", CustomEditorStyles.header1Style);

        EditorGUILayout.PropertyField(goalSet, new GUIContent("AI Goal Set"));

        if (_ai.goalSet == null)
            EditorGUILayout.HelpBox("WARNING: Must assign a goal set for AI to have goals.", MessageType.Warning);
        #endregion

        EditorGUILayout.Space(10.0f);

        #region Debug
        GUILayout.Label("Debug", CustomEditorStyles.header1Style);

        EditorGUI.BeginDisabledGroup(true);

        GUILayout.Label("Actions", CustomEditorStyles.header2Style);
        EditorGUILayout.PropertyField(currentAction);
        EditorGUILayout.PropertyField(availableActions, new GUIContent("Available Actions"));

        EditorGUILayout.Space(5.0f);

        GUILayout.Label("Goals", CustomEditorStyles.header2Style);
        EditorGUILayout.PropertyField(currentGoal);

        EditorGUILayout.Space(5.0f);

        GUILayout.Label("Waypoints", CustomEditorStyles.header2Style);
        EditorGUILayout.PropertyField(currentWaypointIndex);

        EditorGUI.EndDisabledGroup();
        #endregion

        serializedObject.ApplyModifiedProperties();
    }
}
