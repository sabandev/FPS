using UnityEngine;
using UnityEditor;

[InitializeOnLoad]
public static class CustomEditorStyles
{   
    #region Public Properties
    public static GUIStyle header1Style;
    public static GUIStyle header2Style;
    public static GUIStyle titleStyle;
    #endregion

    // Public Static Functions
    public static void InitializeCustomStyles()
    {
        #region Header 1
        header1Style = new GUIStyle(EditorStyles.boldLabel);
        header1Style.fontSize = 16;
        header1Style.normal.textColor = Color.white;
        header1Style.margin = new RectOffset(0, 0, 0, 10);
        header1Style.alignment = TextAnchor.MiddleLeft;
        #endregion

        #region Header 2
        header2Style = new GUIStyle(EditorStyles.boldLabel);
        header2Style.fontSize = 12;
        header2Style.normal.textColor = Color.white;
        header2Style.alignment = TextAnchor.MiddleLeft;
        #endregion

        #region Title
        titleStyle = new GUIStyle(EditorStyles.boldLabel);
        titleStyle.fontSize = 20;
        titleStyle.normal.textColor = Color.white;
        titleStyle.alignment = TextAnchor.MiddleCenter;
        #endregion
    }

    public static void Title(string title, int offset=50)
    {
        EditorGUILayout.Space(10.0f);

        Rect rect = EditorGUILayout.GetControlRect(false, 30);
        float lineWidth = 250;
        float lineHeight = 5;

        // Left line
        EditorGUI.DrawRect(
            new Rect(rect.x - offset, rect.y + rect.height / 2, lineWidth, lineHeight),
            Color.gray
        );

        // Right line
        EditorGUI.DrawRect(
            new Rect(rect.xMax - lineWidth + offset, rect.y + rect.height / 2, lineWidth, lineHeight),
            Color.gray
        );

        EditorGUI.LabelField(rect, $" {title} ", titleStyle);
    }
}
