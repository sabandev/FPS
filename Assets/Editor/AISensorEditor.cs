using UnityEngine;
using UnityEditor;

/// <summary>
/// AISensorEditor.
/// Editor Script.
/// Creates a custom inspector for the AISensor class.
/// </summary>
[CustomEditor(typeof(AISensor))]
public class AISensorEditor : Editor
{
    #region Serialized Properties
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
        CustomEditorStyles.InitializeCustomStyles();
        serializedObject.Update();

        #region Title
        CustomEditorStyles.Title("AI Sensor");
        #endregion

        EditorGUILayout.Space(10.0f);

        #region Vision
        GUILayout.Label("Vision", CustomEditorStyles.header1Style);

        EditorGUILayout.PropertyField(visionDistance, new GUIContent("View Distance"));
        EditorGUILayout.PropertyField(visionAngle, new GUIContent("Viewing Angle"));
        EditorGUILayout.PropertyField(visionHeight, new GUIContent("View Height"));
        EditorGUILayout.PropertyField(visionScanFrequency, new GUIContent("Scan View Frequency"));
        EditorGUILayout.PropertyField(visionConeColor, new GUIContent("View Cone Color"));
        EditorGUILayout.PropertyField(visionTargetLayers, new GUIContent("View Target Layers"));
        EditorGUILayout.PropertyField(visionOcclusionLayers, new GUIContent("View Occlusion Layers"));
        #endregion

        EditorGUILayout.Space(10.0f);

        #region Draw Gizmos
        GUILayout.Label("Draw Gizmos", CustomEditorStyles.header1Style);

        EditorGUILayout.PropertyField(drawViewCone, new GUIContent("Draw View Cone Gizmo"));
        EditorGUILayout.PropertyField(drawInSightGizmos, new GUIContent("Draw In Sight Gizmos"));
        #endregion

        EditorGUILayout.Space(10.0f);

        #region Debug
        GUILayout.Label("Debug", CustomEditorStyles.header1Style);

        EditorGUI.BeginDisabledGroup(true);

        EditorGUILayout.PropertyField(currentlyVisibleTargetObjects);

        EditorGUI.EndDisabledGroup();
        #endregion

        serializedObject.ApplyModifiedProperties();
    }
}
