using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using UnityEditor.SceneManagement;

public class MyFont : ScriptableObject
{
    public Font beforeFont;
    public Font afterFont;
}

public class FontReplacer : EditorWindow
{
    static SerializedProperty sp1;
    static SerializedProperty sp2;

    [MenuItem("Tools/Replace All Fonts")]
    public static void ShowWindow()
    {
        EditorWindow a = EditorWindow.GetWindow(typeof(FontReplacer), true, "Font Replacer");
        var obj = ScriptableObject.CreateInstance<MyFont>();
        var serializedObject = new UnityEditor.SerializedObject(obj);

        sp1 = serializedObject.FindProperty("beforeFont");
        sp2 = serializedObject.FindProperty("afterFont");

        Debug.Log("before font " + sp1.propertyType);
        Debug.Log("after font " + sp2.propertyType);
    }

    void OnGUI()
    {
        EditorGUILayout.PropertyField(sp1, new GUIContent("searching font"));
        EditorGUILayout.PropertyField(sp2, new GUIContent("replacing font"));
        if (GUILayout.Button("Replace All Fonts"))
        {
            Debug.Log("you are trying to replace all fonts to new one");

            var textComponents = Resources.FindObjectsOfTypeAll(typeof(Text)) as Text[];
            foreach (var component in textComponents)
            {
                if (component.font == (sp1.objectReferenceValue as Font))
                {
                    component.font = sp2.objectReferenceValue as Font;
                }
            }
            // ※追記 : シーンに変更があることをUnity側に通知しないと、シーンを切り替えたときに変更が破棄されてしまうので、↓が必要
            EditorSceneManager.MarkAllScenesDirty();
        }
    }
}