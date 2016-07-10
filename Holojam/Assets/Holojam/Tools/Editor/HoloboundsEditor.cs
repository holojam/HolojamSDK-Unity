//HoloboundsEditor.cs
//Created by Aaron C Gaudette on 22.06.16

using UnityEngine;
using UnityEditor;
using Holojam.Network;

namespace Holojam{
	[CustomEditor(typeof(Holobounds))]
	public class HoloboundsEditor : Editor{
		static bool fold = true;
		
		SerializedProperty calibrator, bounds, floor, ceiling;
		void OnEnable(){
			calibrator=serializedObject.FindProperty("calibrator");
			bounds=serializedObject.FindProperty("bounds");
			floor=serializedObject.FindProperty("floor");
			ceiling=serializedObject.FindProperty("ceiling");
		}
		
		public override void OnInspectorGUI(){
			serializedObject.Update();
			
			calibrator.objectReferenceValue=EditorGUILayout.ObjectField(
				"Calibrator",calibrator.objectReferenceValue,typeof(HolojamView),true
			);
			
			Holobounds h = (Holobounds)serializedObject.targetObject;
			
			fold=EditorGUILayout.Foldout(fold,"Corners");
			if(fold){
				for(int i=0;i<4;++i){
					string title = i==0?"Front-Left":i==1?"Front-Right":i==2?"Back-Right":"Back-Left";
					EditorGUILayout.LabelField(title);
					EditorGUILayout.BeginHorizontal();
						bounds.GetArrayElementAtIndex(i).vector2Value=EditorGUILayout.Vector2Field(
							"",bounds.GetArrayElementAtIndex(i).vector2Value,GUILayout.Height(16)
						);
						if(GUILayout.Button("C"))h.Calibrate(i);
					EditorGUILayout.EndHorizontal();
				}
				EditorGUILayout.Space();
			}
			
			EditorGUILayout.BeginHorizontal();
				floor.floatValue=EditorGUILayout.FloatField("Floor",floor.floatValue);
				if(GUILayout.Button("C"))h.Calibrate(4);
			EditorGUILayout.EndHorizontal();
			
			EditorGUILayout.BeginHorizontal();
				ceiling.floatValue=EditorGUILayout.FloatField("Ceiling",ceiling.floatValue);
				if(GUILayout.Button("C"))h.Calibrate(5);
			EditorGUILayout.EndHorizontal();
			
			EditorGUILayout.LabelField(
				"Tracked Space",0.01f*Mathf.Round(100*h.area)+" m²",new GUIStyle(EditorStyles.boldLabel)
			);
			
			serializedObject.ApplyModifiedProperties();
		}
	}
}