// https://forum.unity.com/threads/variables-are-not-saved-with-the-scene-when-using-custom-editor.374984/
// Thanks to Baste.

using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public static class SparkEditorExtensions
{
    public static void SetObjectDirty(Object o)
    {
        EditorUtility.SetDirty(o);
    }

    public static void SetObjectDirty(GameObject go)
    {
        EditorUtility.SetDirty(go);
        EditorSceneManager.MarkSceneDirty(go.scene); //This used to happen automatically from SetDirty
    }

    public static void SetObjectDirty(Component comp)
    {
        EditorUtility.SetDirty(comp);
        EditorSceneManager.MarkSceneDirty(comp.gameObject.scene); //This used to happen automatically from SetDirty
    }
}