//PairableEditor.cs
//Created by Aaron C Gaudette on 24.06.16

using UnityEngine;
using UnityEditor;
using Holojam.Network;

namespace Holojam{
	[CustomEditor(typeof(Pairable)), CanEditMultipleObjects]
	public class PairableEditor : Editor{
		SerializedProperty type, trackingTag, centerOffset;
		void OnEnable(){
			type=serializedObject.FindProperty("type");
			trackingTag=serializedObject.FindProperty("trackingTag");
			centerOffset=serializedObject.FindProperty("centerOffset");
		}
		public override void OnInspectorGUI(){
			serializedObject.Update();
			
			EditorGUILayout.PropertyField(type,new GUIContent("Pairable Type"));
			EditorGUILayout.PropertyField(trackingTag);
			EditorGUILayout.PropertyField(centerOffset,new GUIContent("Center Offset"));
			
			if(!serializedObject.isEditingMultipleObjects){
				EditorGUILayout.Space();
				
				Pairable p = (Pairable)serializedObject.targetObject;
				GUIStyle style = new GUIStyle(EditorStyles.boldLabel);
				EditorGUIUtility.labelWidth=72;
				
				if(p.paired && !p.unpairing)style.normal.textColor=new Color(0.5f,1,1);
				
				EditorGUILayout.LabelField("Status",
					(p.paired && !p.unpairing)?"Paired with \""+p.pairedActor.gameObject.name+"\"":
					(!p.paired && !p.pairing)?"Unpaired":
					p.pairing?"Pairing...":"Unpairing...",
					style
				);
				
				if(Application.isPlaying)
					style.normal.textColor=
						p.managed?new Color(0.5f,1,0.5f):new Color(1,0.5f,0.5f);
				
				EditorGUILayout.LabelField(" ",
					(p.managed?"Managed":"Unmanaged"),
					style
				);
			}
			
			EditorUtility.SetDirty(serializedObject.targetObject);
			serializedObject.ApplyModifiedProperties();
		}
	}
}