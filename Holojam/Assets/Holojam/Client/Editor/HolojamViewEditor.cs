//HolojamViewEditor.cs
//Created by Aaron C Gaudette on 10.07.16

using UnityEngine;
using UnityEditor;
using Holojam.Network;

namespace Holojam{
	[CustomEditor(typeof(HolojamView))]
	public class HolojamViewEditor : Editor{
		public override void OnInspectorGUI(){
			HolojamView hv = (HolojamView)target;
			GUIStyle style = new GUIStyle(EditorStyles.boldLabel);
			
			if(Application.isPlaying)
				style.normal.textColor=
					hv.IsTracked?new Color(0.5f,1,0.5f):new Color(1,0.5f,0.5f);
			
			bool label = hv.Label==null || hv.Label=="";
			EditorGUILayout.LabelField(label?"No Label":hv.Label,
				label?"":
				(hv.IsMine && hv.IsTracked)?"Sending":
				hv.IsMine?"Paused":
				hv.IsTracked?"Tracked":"Untracked",
				style
			);
			
			EditorUtility.SetDirty(serializedObject.targetObject);
		}
	}
}