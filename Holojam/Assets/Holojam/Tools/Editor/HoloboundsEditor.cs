//HoloboundsEditor.cs
//Created by Aaron C Gaudette on 22.06.16

using UnityEngine;
using UnityEditor;

namespace Holojam{
	[CustomEditor(typeof(Holobounds))]
	public class HoloboundsEditor : Editor{
		static bool fold = false;
		
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
				"Calibrator",calibrator.objectReferenceValue,typeof(TrackedObject),true
			);
			
			Holobounds h = (Holobounds)serializedObject.targetObject;
			
			fold=EditorGUILayout.Foldout(fold,"Corners");
			if(fold){
				for(int i=0;i<4;++i){
					EditorGUILayout.BeginHorizontal();
						bounds.GetArrayElementAtIndex(i).vector2Value=
							EditorGUILayout.Vector2Field("",bounds.GetArrayElementAtIndex(i).vector2Value);
						if(GUILayout.Button("C"))h.Calibrate(i);
					EditorGUILayout.EndHorizontal();
				}
			}
			EditorGUILayout.BeginHorizontal();
				floor.floatValue=EditorGUILayout.FloatField("Floor",floor.floatValue);
				if(GUILayout.Button("C"))h.Calibrate(4);
			EditorGUILayout.EndHorizontal();
			EditorGUILayout.BeginHorizontal();
				ceiling.floatValue=EditorGUILayout.FloatField("Ceiling",ceiling.floatValue);
				if(GUILayout.Button("C"))h.Calibrate(5);
			EditorGUILayout.EndHorizontal();
			
			EditorGUILayout.LabelField("Play area:",0.01f*Mathf.Round(100*h.area)+" square meters");
			
			serializedObject.ApplyModifiedProperties();
		}
	}
}