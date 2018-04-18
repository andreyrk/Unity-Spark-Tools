using System;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using GameSparks.RT;

[CustomEditor(typeof(SparkView), true, isFallback = true)]
public class SparkViewEditor : Editor
{
    SparkView targetScript;

    private ReorderableList observedBehaviours;

    public void OnEnable()
    {
        targetScript = (SparkView)target;

        observedBehaviours = new ReorderableList(serializedObject, serializedObject.FindProperty("_observedBehaviours"), true, true, true, true)
        {
            drawHeaderCallback = rect =>
            {
                EditorGUI.LabelField(rect, "Observed Behaviours", EditorStyles.boldLabel);
            },

            drawElementCallback =
            (Rect rect, int index, bool isActive, bool isFocused) =>
            {
                var element = observedBehaviours.serializedProperty.GetArrayElementAtIndex(index);
                EditorGUI.ObjectField(new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight), element, GUIContent.none);
            }
        };
    }

    public override void OnInspectorGUI()
    {
        EditorGUILayout.Space();

        targetScript.observeMethod = (GameSparksRT.DeliveryIntent)EditorGUILayout.EnumPopup("Observe Method", targetScript.observeMethod);
        targetScript.sendRate = EditorGUILayout.IntSlider("Send rate", targetScript.sendRate, 1, 100);

        EditorGUILayout.Space();

        serializedObject.Update();
        observedBehaviours.DoLayoutList();
        serializedObject.ApplyModifiedProperties();

        if (GUI.changed)
        {
            SparkEditorExtensions.SetObjectDirty(targetScript);
        }
    }
}
