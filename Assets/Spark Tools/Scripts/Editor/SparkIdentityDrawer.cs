using UnityEditor;
using UnityEngine;
using System;

[CustomPropertyDrawer (typeof(SparkIdentityAttribute))]
public class UniqueIdentifierDrawer : PropertyDrawer
{
	public override void OnGUI (Rect position, SerializedProperty prop, GUIContent label)
	{
		if (prop.stringValue == "") {
			Guid guid = Guid.NewGuid ();
			prop.stringValue = guid.ToString ();
		}

		Rect textFieldPosition = position;
		textFieldPosition.height = 16;

		EditorGUI.LabelField (position, label, new GUIContent (prop.stringValue));
	}
}