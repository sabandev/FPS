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
    SerializedProperty visionDistance;
    SerializedProperty visionAngle;
    SerializedProperty visionHeight;
    SerializedProperty visionScanFrequency;
    SerializedProperty visionConeColor;
    SerializedProperty visionTargetLayers;
    SerializedProperty visionOcclusionLayers;
    SerializedProperty drawViewCone;
    SerializedProperty drawInSightGizmos;
    SerializedProperty currentlyVisibleTargetObjects;
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
        visionDistance = serializedObject.FindProperty("visionDistance");
        visionAngle = serializedObject.FindProperty("visionAngle");
        visionHeight = serializedObject.FindProperty("visionHeight");
        visionScanFrequency = serializedObject.FindProperty("visionScanFrequency");
        visionConeColor = serializedObject.FindProperty("visionConeColor");
        visionTargetLayers = serializedObject.FindProperty("visionTargetLayers");
        visionOcclusionLayers = serializedObject.FindProperty("visionOcclusionLayers");
        drawViewCone = serializedObject.FindProperty("drawViewCone");
        drawInSightGizmos = serializedObject.FindProperty("drawInSightGizmos");
        currentlyVisibleTargetObjects = serializedObject.FindProperty("currentlyVisibleTargetObjects");
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

        #region Senses
        GUILayout.Label("Senses", CustomEditorStyles.header1Style);

        GUILayout.Label("Vision", CustomEditorStyles.header2Style);
        EditorGUILayout.PropertyField(visionDistance, new GUIContent("View Distance"));
        EditorGUILayout.PropertyField(visionAngle, new GUIContent("Viewing Angle"));
        EditorGUILayout.PropertyField(visionHeight, new GUIContent("View Height"));
        EditorGUILayout.PropertyField(visionScanFrequency, new GUIContent("Scan View Frequency"));
        EditorGUILayout.PropertyField(visionConeColor, new GUIContent("View Cone Color"));
        EditorGUILayout.PropertyField(visionTargetLayers, new GUIContent("View Target Layers"));
        EditorGUILayout.PropertyField(visionOcclusionLayers, new GUIContent("View Occlusion Layers"));

        EditorGUILayout.Space(5.0f);

        GUILayout.Label("Draw Gizmos", CustomEditorStyles.header2Style);
        EditorGUILayout.PropertyField(drawViewCone, new GUIContent("Draw View Cone Gizmo"));
        EditorGUILayout.PropertyField(drawInSightGizmos, new GUIContent("Draw In Sight Gizmos"));
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

        GUILayout.Label("Targets", CustomEditorStyles.header2Style);
        EditorGUILayout.PropertyField(assignTargetGO, new GUIContent("Target Destination"));
        if (_ai.assignTargetGameObject)
        {
            EditorGUILayout.PropertyField(targetGO, new GUIContent(""));
        }
        else
            _ai.target = null;

        EditorGUILayout.Space(5.0f);

        EditorGUILayout.PropertyField(assignWaypoints, new GUIContent("Target Waypoints"));
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

        EditorGUILayout.Space(5.0f);

        GUILayout.Label("Vision", CustomEditorStyles.header2Style);
        EditorGUILayout.PropertyField(currentlyVisibleTargetObjects);

        EditorGUI.EndDisabledGroup();
        #endregion

        serializedObject.ApplyModifiedProperties();
    }
}
