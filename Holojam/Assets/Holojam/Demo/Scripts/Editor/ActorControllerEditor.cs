//ActorControllerEditor.cs
//Created by Aaron C Gaudette on 05.07.16

using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ActorController)), CanEditMultipleObjects]
public class ActorControllerEditor : Holojam.Tools.ActorEditor{
	SerializedProperty head;
	protected override void EnableDerived(){
		head=serializedObject.FindProperty("head");
	}
	protected override void DrawDerived(){
		EditorGUILayout.PropertyField(head);
	}
}