//HoloboundsEditor.cs
//Created by Aaron C Gaudette on 22.06.16

using UnityEngine;
using UnityEditor;
using Holojam.Network;

namespace Holojam.Tools{
	[CustomEditor(typeof(Holobounds))]
	public class HoloboundsEditor : Editor{
		static bool fold = true;
		
		SerializedProperty localSpace, calibrator, bounds, floor, ceiling;
		void OnEnable(){
			localSpace=serializedObject.FindProperty("localSpace");
			calibrator=serializedObject.FindProperty("calibrator");
			bounds=serializedObject.FindProperty("bounds");
			floor=serializedObject.FindProperty("floor");
			ceiling=serializedObject.FindProperty("ceiling");
		}
		
		public override void OnInspectorGUI(){
			serializedObject.Update();
			
			EditorGUILayout.PropertyField(localSpace);
			EditorGUILayout.PropertyField(calibrator);
			
			Holobounds h = (Holobounds)serializedObject.targetObject;
			
			fold=EditorGUILayout.Foldout(fold,"Corners");
			if(fold){
				for(int i=0;i<4;++i){
					string title = i==0?"Front-Left":i==1?"Front-Right":i==2?"Back-Right":"Back-Left";
					EditorGUILayout.LabelField(title);
					EditorGUILayout.BeginHorizontal();
						EditorGUILayout.PropertyField(
							bounds.GetArrayElementAtIndex(i),
							new GUIContent(""),GUILayout.Height(16)
						);
						if(GUILayout.Button("C"))h.Calibrate(i);
					EditorGUILayout.EndHorizontal();
				}
				EditorGUILayout.Space();
			}
			
			EditorGUILayout.BeginHorizontal();
				EditorGUILayout.PropertyField(floor);
				if(GUILayout.Button("C"))h.Calibrate(4);
			EditorGUILayout.EndHorizontal();
			
			EditorGUILayout.BeginHorizontal();
				EditorGUILayout.PropertyField(ceiling);
				if(GUILayout.Button("C"))h.Calibrate(5);
			EditorGUILayout.EndHorizontal();
			
			EditorGUILayout.LabelField(
				"Tracked Space",0.01f*Mathf.Round(100*h.area)+" m²",new GUIStyle(EditorStyles.boldLabel)
			);
			
			EditorUtility.SetDirty(serializedObject.targetObject);
			serializedObject.ApplyModifiedProperties();
		}
	}
}