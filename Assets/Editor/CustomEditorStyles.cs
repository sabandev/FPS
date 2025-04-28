using UnityEngine;
using UnityEditor;
using Unity.VisualScripting;

[InitializeOnLoad]
public static class CustomEditorStyles
{   
    #region Public Properties
    public static GUIStyle header1Style;
    public static GUIStyle header2Style;
    public static GUIStyle titleStyle;
    public static GUIStyle foldoutHeader1Style;
    #endregion

    // Public Static Functions
    public static void InitializeCustomStyles()
    {
        int _header1FontSize = 18;

        #region Header 1
        header1Style = new GUIStyle(EditorStyles.boldLabel);
        header1Style.fontSize = _header1FontSize;
        header1Style.normal.textColor = Color.white;
        header1Style.margin = new RectOffset(0, 0, 0, 10);
        header1Style.alignment = TextAnchor.MiddleLeft;
        #endregion

        #region Header 2
        header2Style = new GUIStyle(EditorStyles.boldLabel);
        header2Style.fontSize = 13;
        header2Style.normal.textColor = Color.white;
        header2Style.alignment = TextAnchor.MiddleLeft;
        #endregion

        #region Title
        titleStyle = new GUIStyle(EditorStyles.boldLabel);
        titleStyle.fontSize = 25;
        titleStyle.normal.textColor = Color.white;
        titleStyle.alignment = TextAnchor.MiddleCenter;
        #endregion

        #region Foldout Header 1
        foldoutHeader1Style = new GUIStyle(EditorStyles.foldoutHeader);
        foldoutHeader1Style.fontSize = _header1FontSize;
        foldoutHeader1Style.margin = new RectOffset(0, 0, 0, 10);
        foldoutHeader1Style.alignment = TextAnchor.MiddleCenter;
        foldoutHeader1Style.fontStyle = FontStyle.Bold;

        foldoutHeader1Style.normal.textColor = Color.white;
        foldoutHeader1Style.hover.textColor = Color.white;
        foldoutHeader1Style.focused.textColor = Color.white;
        foldoutHeader1Style.active.textColor = Color.white;

        foldoutHeader1Style.onNormal.textColor = Color.white;
        foldoutHeader1Style.onHover.textColor = Color.white;
        foldoutHeader1Style.onFocused.textColor = Color.white;
        foldoutHeader1Style.onActive.textColor = Color.white;
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
