//HolojamNetworkEditor.cs
//Created by Aaron C Gaudette on 09.07.16
//Simple editor script to display packet data

using UnityEngine;
using UnityEditor;
using Holojam.Network;

namespace Holojam{
	[CustomEditor(typeof(HolojamNetwork))]
	public class HolojamNetworkEditor : Editor{
		SerializedProperty sentWarning, receivedWarning;
		void OnEnable(){
			sentWarning=serializedObject.FindProperty("sentWarning");
			receivedWarning=serializedObject.FindProperty("receivedWarning");
		}
		public override void OnInspectorGUI(){
			serializedObject.Update();
			
			EditorGUIUtility.labelWidth=64;
			GUIStyle style = new GUIStyle(EditorStyles.boldLabel);
			
			EditorGUILayout.LabelField("Warning Threshold",style);
			EditorGUILayout.BeginHorizontal();
				sentWarning.intValue=EditorGUILayout.IntField("Sent",Mathf.Max(sentWarning.intValue,-1));
				receivedWarning.intValue=EditorGUILayout.IntField("Received",Mathf.Max(receivedWarning.intValue,-1));
			EditorGUILayout.EndHorizontal();
			
			HolojamNetwork hj = (HolojamNetwork)serializedObject.targetObject;
			
			EditorGUILayout.LabelField("Packets per Second",style);
			EditorGUILayout.BeginHorizontal();
				if(Application.isPlaying)
					style.normal.textColor=hj.sentPPS>sentWarning.intValue?
						new Color(0.5f,1,0.5f):new Color(1,0.5f,0.5f);
				EditorGUILayout.LabelField("Sent",hj.sentPPS.ToString(),style);
				
				if(Application.isPlaying)
					style.normal.textColor=hj.receivedPPS>receivedWarning.intValue?
						new Color(0.5f,1,0.5f):new Color(1,0.5f,0.5f);
				EditorGUILayout.LabelField("Received",hj.receivedPPS.ToString(),style);
			EditorGUILayout.EndHorizontal();
			
			serializedObject.ApplyModifiedProperties();
		}
	}
}