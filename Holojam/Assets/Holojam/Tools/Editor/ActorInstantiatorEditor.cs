//ActorInstantiatorEditor.cs
//Created by Aaron C Gaudette on 22.06.16

using UnityEngine;
using UnityEditor;

namespace Holojam{
	[CustomEditor(typeof(ActorInstantiator))]
	public class ActorInstantiatorEditor : Editor{
		public override void OnInspectorGUI(){
			ActorInstantiator ai = (ActorInstantiator)target;
			
			ai.actor=EditorGUILayout.ObjectField("Actor",ai.actor,typeof(Actor),false) as Actor;
			ai.amount=EditorGUILayout.IntField("Amount",ai.amount);
			
			if(GUILayout.Button("Add Actors"))ai.Add();
			if(GUILayout.Button("Clear Actors"))ai.Clear();
		}
	}
}