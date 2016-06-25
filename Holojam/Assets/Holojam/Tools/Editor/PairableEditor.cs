//PairableEditor.cs
//Created by Aaron C Gaudette on 24.06.16

using UnityEngine;
using UnityEditor;

namespace Holojam{
	[CustomEditor(typeof(Pairable))]
	public class PairableEditor : Editor{
		public override void OnInspectorGUI(){
			Pairable p = (Pairable)target;
			p.type=EditorGUILayout.TextField("Pairable Type",p.type);
			p.offset=EditorGUILayout.Vector3Field("Offset",p.offset);
			EditorGUILayout.LabelField("Status:",
				(p.paired && !p.unpairing)?"Paired with "+p.pairedActor.gameObject.name:
				(!p.paired && !p.pairing)?"Unpaired":
				p.pairing?"Pairing...":"Unpairing..."
			);
		}
	}
}