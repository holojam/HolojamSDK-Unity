//HoloboundsEditor.cs
//Created by Aaron C Gaudette on 22.06.16

using UnityEngine;
using UnityEditor;

namespace Holojam{
	[CustomEditor(typeof(Holobounds))]
	public class HoloboundsEditor : Editor{
		bool fold = false;
		public override void OnInspectorGUI(){
			Holobounds h = (Holobounds)target;
			
			h.calibrator=EditorGUILayout.ObjectField(
				"Calibrator",h.calibrator,typeof(TrackedObject),false
			) as TrackedObject;
			
			fold=EditorGUILayout.Foldout(fold,"Corners");
			if(fold){
				for(int i=0;i<4;++i){
					EditorGUILayout.BeginHorizontal();
						h.bounds[i]=EditorGUILayout.Vector2Field("",h.bounds[i]);
						if(GUILayout.Button("C"))h.Calibrate(i);
					EditorGUILayout.EndHorizontal();
				}
			}
			EditorGUILayout.BeginHorizontal();
				h.floor=EditorGUILayout.FloatField("Floor",h.floor);
				if(GUILayout.Button("C"))h.Calibrate(4);
			EditorGUILayout.EndHorizontal();
		}
	}
}