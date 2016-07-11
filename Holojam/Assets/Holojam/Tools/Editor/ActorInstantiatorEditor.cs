//ActorInstantiatorEditor.cs
//Created by Aaron C Gaudette on 22.06.16

using UnityEngine;
using UnityEditor;

namespace Holojam{
	[CustomEditor(typeof(ActorInstantiator))]
	public class ActorInstantiatorEditor : Editor{
		SerializedProperty actor, amount;
		void OnEnable(){
			actor=serializedObject.FindProperty("actor");
			amount=serializedObject.FindProperty("amount");
		}
		public override void OnInspectorGUI(){
			serializedObject.Update();
			
			GUILayout.BeginHorizontal();
				EditorGUIUtility.labelWidth=38;
				EditorGUILayout.PropertyField(actor);
				EditorGUIUtility.labelWidth=52;
				EditorGUILayout.PropertyField(amount);
			GUILayout.EndHorizontal();
			
			ActorInstantiator ai = (ActorInstantiator)serializedObject.targetObject;
			
			if(GUILayout.Button("Add",GUILayout.Height(20)))ai.Add();
			if(GUILayout.Button("Clear All",GUILayout.Height(20)))ai.Clear();
			
			EditorUtility.SetDirty(serializedObject.targetObject);
			EditorUtility.SetDirty(ai.GetComponent<ActorManager>());
			serializedObject.ApplyModifiedProperties();
		}
	}
}