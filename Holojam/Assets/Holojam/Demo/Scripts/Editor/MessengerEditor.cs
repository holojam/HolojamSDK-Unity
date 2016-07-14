//MessengerEditor.cs
//Created by Aaron C Gaudette on 13.07.16

using UnityEngine;
using UnityEditor;

namespace Holojam{
	[CustomEditor(typeof(Messenger))]
	public class MessengerEditor : Editor{
		string message = "";
		public override void OnInspectorGUI(){
			DrawDefaultInspector();
			
			Messenger m = (Messenger)serializedObject.targetObject;
			
			message=EditorGUILayout.TextField(message);
			if(GUILayout.Button("Push",GUILayout.Height(20))){
				m.Push(message);
				message="";
			}
		}
	}
}