using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering.UI;

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
    SerializedProperty updatePathCooldown;
    SerializedProperty vision;
    #endregion

    #region Private Properties
    private string _AITypeName = "Unspecified";

    private float _spaceBetweenCategories = 20.0f;

    private bool ShowType
    {
        get => EditorPrefs.GetBool(nameof(_showType), false);
        set => EditorPrefs.SetBool(nameof(_showType), value);
    }

    private bool ShowSenses
    {
        get => EditorPrefs.GetBool(nameof(_showSenses), false);
        set => EditorPrefs.SetBool(nameof(_showSenses), value);
    }

    private bool ShowNavigation
    {
        get => EditorPrefs.GetBool(nameof(_showNavigation), false);
        set => EditorPrefs.SetBool(nameof(_showNavigation), value);
    }

    private bool ShowPlanning
    {
        get => EditorPrefs.GetBool(nameof(_showPlanning), false);
        set => EditorPrefs.SetBool(nameof(_showPlanning), value);
    }

    private bool ShowDebug
    {
        get => EditorPrefs.GetBool(nameof(_showDebug), true);
        set => EditorPrefs.SetBool(nameof(_showDebug), value);
    }

    private bool _showType;
    private bool _showSenses;
    private bool _showNavigation;
    private bool _showPlanning;
    private bool _showDebug;
    private bool _notAutoOpenedDebugFoldout = true;
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
        updatePathCooldown = serializedObject.FindProperty("updatePathCooldown");
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
        ShowType = EditorGUILayout.BeginFoldoutHeaderGroup(ShowType, new GUIContent("Type"), CustomEditorStyles.foldoutHeader1Style);

        if (ShowType)
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
        ShowSenses = EditorGUILayout.BeginFoldoutHeaderGroup(ShowSenses, new GUIContent("Senses"), CustomEditorStyles.foldoutHeader1Style);
        
        if (ShowSenses)
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
        ShowNavigation = EditorGUILayout.BeginFoldoutHeaderGroup(ShowNavigation, new GUIContent("Navigation"), CustomEditorStyles.foldoutHeader1Style);

        if(ShowNavigation)
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
            EditorGUILayout.PropertyField(updatePathCooldown);

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
        ShowPlanning = EditorGUILayout.BeginFoldoutHeaderGroup(ShowPlanning, new GUIContent("Planning"), CustomEditorStyles.foldoutHeader1Style);

        if (ShowPlanning)
        {
            GUILayout.Label("Goals", CustomEditorStyles.header2Style);

            EditorGUILayout.PropertyField(goalSet, new GUIContent("AI Goal Set"));

            if (_ai.goalSet == null)
                EditorGUILayout.HelpBox("Error: Must assign a goal set. The AI must have goals.", MessageType.Error);

            EditorGUILayout.Space(5.0f);

            GUILayout.Label("Actions", CustomEditorStyles.header2Style);

        }
        
        EditorGUILayout.EndFoldoutHeaderGroup();
        #endregion

        EditorGUILayout.Space(_spaceBetweenCategories);

        #region Debug
        if (EditorApplication.isPlaying && _notAutoOpenedDebugFoldout)
        {
            _notAutoOpenedDebugFoldout = false;
            ShowDebug = true;
        }

        ShowDebug = EditorGUILayout.BeginFoldoutHeaderGroup(ShowDebug, new GUIContent("Debug"), CustomEditorStyles.foldoutHeader1Style);

        if (ShowDebug)
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
