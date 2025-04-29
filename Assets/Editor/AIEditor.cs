using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

/// <summary>
/// AIEditor.
/// Editor Script.
/// Creates a custom inspector for the AI class.
/// </summary>
[CustomEditor(typeof(AI))]
[CanEditMultipleObjects]
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
    SerializedProperty goToActionUpdateCooldown;
    SerializedProperty vision;
    #endregion

    #region Private Properties
    private string _AITypeName = "Unspecified";

    private float _spaceBetweenCategories = 20.0f;

    private bool _showType = false;
    private bool _showSenses = false;
    private bool _showNavigation = false;
    private bool _showPlanning = false;
    private bool _showDebug = false;
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
        goToActionUpdateCooldown = serializedObject.FindProperty("goToActionUpdateCooldown");
        vision = serializedObject.FindProperty("vision");
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

        GUILayout.Label(_AITypeName, CustomEditorStyles.titleStyle);

        EditorGUILayout.Space(_spaceBetweenCategories + 10.0f);

        #region Type
        _showType = EditorGUILayout.BeginFoldoutHeaderGroup(_showType, new GUIContent("Type"), CustomEditorStyles.foldoutHeader1Style);

        if (_showType)
        {
            EditorGUILayout.ObjectField(aiType, new GUIContent(""));
            
            if (_ai.aiType == null)
            {
                EditorGUILayout.HelpBox("ERROR: Must assign an AI type.", MessageType.Error);
            }
        }

        if (_ai.aiType != null)
            _AITypeName = _ai.aiType.name;
        else
            _AITypeName = "Unspecified";

        EditorGUILayout.EndFoldoutHeaderGroup();
        #endregion

        EditorGUILayout.Space(_spaceBetweenCategories);

        #region Senses
        _showSenses = EditorGUILayout.BeginFoldoutHeaderGroup(_showSenses, new GUIContent("Senses"), CustomEditorStyles.foldoutHeader1Style);
        
        if (_showSenses)
        {
            EditorGUILayout.BeginHorizontal();

            GUILayout.Label("Vision", CustomEditorStyles.header2Style);
            
            GUILayout.FlexibleSpace();

            EditorGUILayout.PropertyField(vision, new GUIContent(""), GUILayout.Width(20));

            EditorGUILayout.EndHorizontal();

            if (!_ai.vision)
                EditorGUI.BeginDisabledGroup(true);

            EditorGUILayout.Slider(visionDistance, 0.0f, 100.0f, new GUIContent("View Distance"));
            EditorGUILayout.Slider(visionAngle, 0.0f, 90.0f, new GUIContent("Viewing Angle"));
            EditorGUILayout.PropertyField(visionHeight, new GUIContent("View Height"));
            EditorGUILayout.PropertyField(visionScanFrequency, new GUIContent("Scan View Frequency"));
            EditorGUILayout.PropertyField(visionConeColor, new GUIContent("View Cone Color"));
            EditorGUILayout.PropertyField(visionTargetLayers, new GUIContent("View Target Layers"));
            EditorGUILayout.PropertyField(visionOcclusionLayers, new GUIContent("View Occlusion Layers"));

            EditorGUILayout.Space(5.0f);

            GUILayout.Label("Draw Gizmos", CustomEditorStyles.header2Style);
            EditorGUILayout.PropertyField(drawViewCone, new GUIContent("Draw View Cone Gizmo"));
            EditorGUILayout.PropertyField(drawInSightGizmos, new GUIContent("Draw In Sight Gizmos"));

            if (!_ai.vision)
                EditorGUI.EndDisabledGroup();
        }

        EditorGUILayout.EndFoldoutHeaderGroup();
        #endregion

        EditorGUILayout.Space(_spaceBetweenCategories);

        #region Navigation
        _showNavigation = EditorGUILayout.BeginFoldoutHeaderGroup(_showNavigation, new GUIContent("Navigation"), CustomEditorStyles.foldoutHeader1Style);

        if(_showNavigation)
        {
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

            EditorGUILayout.Space(5.0f);

            EditorGUILayout.PropertyField(assignWaypoints, new GUIContent("Target Waypoints"));

            if (_ai.assignWaypoints)
            {
                for (int i = 0; i < waypoints.arraySize; i++)
                {
                    EditorGUILayout.BeginHorizontal();

                    EditorGUILayout.PropertyField(waypoints.GetArrayElementAtIndex(i));

                    if (GUILayout.Button("X", GUILayout.Width(20)))
                    {
                        waypoints.DeleteArrayElementAtIndex(i);
                        break;
                    }

                    EditorGUILayout.EndHorizontal();
                }

                EditorGUILayout.Space(5.0f);

                if (GUILayout.Button("Add Waypoint"))
                {
                    waypoints.arraySize++;
                    SerializedProperty newWaypoint = waypoints.GetArrayElementAtIndex(waypoints.arraySize - 1);
                    newWaypoint.objectReferenceValue = null;
                }
            }

        }

        EditorGUILayout.EndFoldoutHeaderGroup();
        #endregion

        EditorGUILayout.Space(_spaceBetweenCategories);

        #region Planning
        _showPlanning = EditorGUILayout.BeginFoldoutHeaderGroup(_showPlanning, new GUIContent("Planning"), CustomEditorStyles.foldoutHeader1Style);

        if (_showPlanning)
        {
            GUILayout.Label("Goals", CustomEditorStyles.header2Style);

            EditorGUILayout.PropertyField(goalSet, new GUIContent("AI Goal Set"));

            if (_ai.goalSet == null)
                EditorGUILayout.HelpBox("Error: Must assign a goal set. The AI must have goals.", MessageType.Error);

            EditorGUILayout.Space(5.0f);

            GUILayout.Label("Actions", CustomEditorStyles.header2Style);

            EditorGUILayout.PropertyField(goToActionUpdateCooldown, new GUIContent("Update GoTo Actions Delay"));
        }
        
        EditorGUILayout.EndFoldoutHeaderGroup();
        #endregion

        EditorGUILayout.Space(_spaceBetweenCategories);

        #region Debug
        _showDebug = EditorGUILayout.BeginFoldoutHeaderGroup(_showDebug, new GUIContent("Debug"), CustomEditorStyles.foldoutHeader1Style);

        if (_showDebug)
        {
            EditorGUI.BeginDisabledGroup(true);

            GUILayout.Label("Current Action", CustomEditorStyles.header2Style);
            EditorGUILayout.PropertyField(currentAction, new GUIContent("Performing:"));

            EditorGUILayout.Space(5.0f);

            GUILayout.Label("Available Actions", CustomEditorStyles.header2Style);

            if (_ai.availableActions.Count == 0)
                GUILayout.Label("No available actions");

            if (_ai.availableActions != null)
            {
                for (int i = 0; i < _ai.availableActions.Count; i++)
                {
                    _ai.availableActions[i] = (GOAP_Action)EditorGUILayout.ObjectField(
                        "",
                        _ai.availableActions[i],
                        typeof(GOAP_Action),
                        true
                    );
                }
            }

            EditorGUILayout.Space(5.0f);

            GUILayout.Label("Current Goal", CustomEditorStyles.header2Style);

            if (_ai.currentGoal.goalName != string.Empty)
                EditorGUILayout.PropertyField(currentGoal, new GUIContent($"{_ai.currentGoal.goalName}"), true);
            else
                GUILayout.Label("No current goal");

            EditorGUILayout.Space(5.0f);

            GUILayout.Label("Waypoints", CustomEditorStyles.header2Style);
            EditorGUILayout.PropertyField(currentWaypointIndex);

            EditorGUILayout.Space(5.0f);

            GUILayout.Label("Visible Target Objects", CustomEditorStyles.header2Style);

            if (_ai.currentlyVisibleTargetObjects.Count == 0)
                GUILayout.Label("No target objects visisble");
            
            if (_ai.currentlyVisibleTargetObjects != null)
            {
                for (int i = 0; i < _ai.currentlyVisibleTargetObjects.Count; i++)
                {
                    _ai.currentlyVisibleTargetObjects[i] = (GameObject)EditorGUILayout.ObjectField(
                        "",
                        _ai.currentlyVisibleTargetObjects[i],
                        typeof(GameObject),
                        true
                    );
                }
            }

            EditorGUI.EndDisabledGroup();
        }

        EditorGUILayout.EndFoldoutHeaderGroup();
        #endregion

        serializedObject.ApplyModifiedProperties();
    }
}
