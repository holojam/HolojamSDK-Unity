//ActorControllerEditor.cs
//Created by Aaron C Gaudette on 05.07.16

using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ActorController))]
public class ActorControllerEditor : Holojam.ActorEditor{
	SerializedProperty head;
	protected override void EnableDerived(){
		head=serializedObject.FindProperty("head");
	}
	protected override void DrawDerived(){
		head.objectReferenceValue=
			EditorGUILayout.ObjectField("Head",head.objectReferenceValue,typeof(Transform),true);
	}
}