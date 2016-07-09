//ActorEditor.cs
//Created by Aaron C Gaudette on 05.07.16

using UnityEngine;
using UnityEditor;

namespace Holojam{
	[CustomEditor(typeof(Actor)), CanEditMultipleObjects]
	public class ActorEditor : Editor{
		//Override these functions if you desire a custom inspector
		protected virtual void EnableDerived(){}
		protected virtual void DrawDerived(){}
		
		SerializedProperty handle, motif, mask;
		void OnEnable(){
			handle=serializedObject.FindProperty("handle");
			motif=serializedObject.FindProperty("motif");
			mask=serializedObject.FindProperty("mask");
			
			EnableDerived();
		}
		public override void OnInspectorGUI(){
			serializedObject.Update();
			
			EditorGUILayout.BeginHorizontal();
				handle.stringValue=EditorGUILayout.TextField(handle.stringValue);
				motif.colorValue=EditorGUILayout.ColorField(motif.colorValue);
			EditorGUILayout.EndHorizontal();
			
			mask.objectReferenceValue=
				EditorGUILayout.ObjectField("Mask",mask.objectReferenceValue,typeof(GameObject),true);
			
			DrawDerived();
			
			if(!serializedObject.isEditingMultipleObjects){
				Actor a = serializedObject.targetObject as Actor;
				EditorStyles.label.wordWrap = true;
				EditorGUILayout.LabelField(
					"Actor "+(a.index+1)+" ("+(a.managed?"Managed/":"Unmanaged/")+(a.view.IsTracked?"Tracked)":"Untracked)")
				);
			}
			
			serializedObject.ApplyModifiedProperties();
		}
	}
}