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

    #region Private Properties
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
        goals = serializedObject.FindProperty("goals");
        worldStates = serializedObject.FindProperty("worldStates");
        #endregion
    }

    public override void OnInspectorGUI()
    {
        InitialiseCustomStyles();
        serializedObject.Update();

        Title("World");

        EditorGUILayout.Space(10.0f);

        GUILayout.Label("World States", _header1Style);
        EditorGUILayout.PropertyField(worldStates);

        EditorGUILayout.Space(10.0f);

        GUILayout.Label("Debug", _header1Style);

        EditorGUI.BeginDisabledGroup(true);

        EditorGUILayout.PropertyField(goals, new GUIContent("World Goals"));

        EditorGUI.EndDisabledGroup();

        serializedObject.ApplyModifiedProperties();
    }

    private void Title(string title)
    {
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

        EditorGUI.LabelField(rect, $" {title} ", _titleStyle);
    }
}
