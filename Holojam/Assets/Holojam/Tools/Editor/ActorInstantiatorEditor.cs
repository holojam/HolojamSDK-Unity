//ActorInstantiatorEditor.cs
//Created by Aaron C Gaudette on 22.06.16

using UnityEngine;
using UnityEditor;

namespace Holojam{
	[CustomEditor(typeof(ActorInstantiator))]
	public class ActorInstantiatorEditor : Editor{
		public override void OnInspectorGUI(){
			ActorInstantiator ai = (ActorInstantiator)target;
			
			GUILayout.BeginHorizontal();
			EditorGUIUtility.labelWidth=38;
			ai.actor=EditorGUILayout.ObjectField("Actor",ai.actor,typeof(Actor),false) as Actor;
			EditorGUIUtility.labelWidth=52;
			ai.amount=EditorGUILayout.IntField("Amount",Mathf.Max(ai.amount,0));
			GUILayout.EndHorizontal();
			
			if(GUILayout.Button("Add"))ai.Add();
			if(GUILayout.Button("Clear All"))ai.Clear();
		}
	}
}