//ActorEditor.cs
//Created by Aaron C Gaudette on 05.07.16

using UnityEngine;
using UnityEditor;
using Holojam.Network;

namespace Holojam{
	[CustomEditor(typeof(Actor)), CanEditMultipleObjects]
	public class ActorEditor : Editor{
		//Override these functions if you desire a custom inspector
		protected virtual void EnableDerived(){}
		protected virtual void DrawDerived(){}
		
		SerializedProperty handle, motif, trackingTag, mask;
		void OnEnable(){
			handle=serializedObject.FindProperty("handle");
			motif=serializedObject.FindProperty("motif");
			trackingTag=serializedObject.FindProperty("trackingTag");
			mask=serializedObject.FindProperty("mask");
			
			EnableDerived();
		}
		public override void OnInspectorGUI(){
			serializedObject.Update();
			
			EditorGUILayout.BeginHorizontal();
				//Unity has no proper window width accessor, so this will offset marginally when scrolling
				EditorGUILayout.PropertyField(handle,new GUIContent(""),GUILayout.Width(EditorGUIUtility.labelWidth-4));
				EditorGUILayout.PropertyField(trackingTag,new GUIContent(""));
				EditorGUILayout.PropertyField(motif,new GUIContent(""),GUILayout.Width(48));
			EditorGUILayout.EndHorizontal();
			
			EditorGUILayout.PropertyField(mask);
			DrawDerived();
			
			if(!serializedObject.isEditingMultipleObjects){
				Actor a = serializedObject.targetObject as Actor;
				
				GUIStyle style = new GUIStyle(EditorStyles.boldLabel);
				if(Application.isPlaying)
					style.normal.textColor=a.managed?new Color(0.5f,1,0.5f):new Color(1,0.5f,0.5f);
				
				EditorGUILayout.LabelField("Status",
					(a.managed?"Managed":"Unmanaged"),
					style
				);
				
				if(a.managed && !a.manager.runtimeIndexing && Application.isPlaying)
					EditorGUILayout.LabelField(
						"Runtime indexing is OFF. Actor will not reflect changes under manager during playmode.",
						new GUIStyle(EditorStyles.wordWrappedMiniLabel)
					);
			}
			
			serializedObject.ApplyModifiedProperties();
		}
	}
}