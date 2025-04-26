using UnityEngine;
using UnityEditor;
using System.Net;

/// <summary>
/// GOAP_WorldEditor.
/// Editor Script.
/// Creates a custom inspector for the GOAP_World class.
/// </summary>
[CustomEditor(typeof(GOAP_World))]
public class GOAP_WorldEditor : Editor
{
    #region Serialized Properties
    SerializedProperty goals;
    SerializedProperty worldStates;
    #endregion

    private void OnEnable()
    {
        #region Set Serialized Properties
        goals = serializedObject.FindProperty("goals");
        worldStates = serializedObject.FindProperty("worldStates");
        #endregion
    }

    public override void OnInspectorGUI()
    {
        CustomEditorStyles.InitializeCustomStyles();
        serializedObject.Update();

        CustomEditorStyles.Title("World");

        EditorGUILayout.Space(10.0f);

        GUILayout.Label("World States", CustomEditorStyles.header1Style);
        EditorGUILayout.PropertyField(worldStates);

        EditorGUILayout.Space(10.0f);

        GUILayout.Label("Debug", CustomEditorStyles.header1Style);

        EditorGUI.BeginDisabledGroup(true);

        EditorGUILayout.PropertyField(goals, new GUIContent("World Goals"));

        EditorGUI.EndDisabledGroup();

        serializedObject.ApplyModifiedProperties();
    }
}
