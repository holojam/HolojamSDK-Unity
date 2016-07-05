//ActorEditor.cs
//Created by Aaron C Gaudette on 05.07.16

using UnityEngine;
using UnityEditor;

namespace Holojam{
	[CustomEditor(typeof(Actor))]
	public class ActorEditor : Editor{
		public override void OnInspectorGUI(){
			Actor a = (Actor)target;
			
			EditorGUILayout.BeginHorizontal();
				a.name=EditorGUILayout.TextField(a.name);
				a.motif=EditorGUILayout.ColorField(a.motif);
			EditorGUILayout.EndHorizontal();
			
			a.liveObjectTag=(Holojam.Server.LiveObjectTag)EditorGUILayout.EnumPopup("Headset",a.liveObjectTag);
			
			a.mask=EditorGUILayout.ObjectField("Mask",a.mask,typeof(GameObject),true) as GameObject;
			
			EditorStyles.label.wordWrap = true;
			EditorGUILayout.LabelField(
				"Actor "+(a.index+1)+" ("+(a.managed?"Managed/":"Unmanaged/")+(a.tracking?"Tracked)":"Untracked)")
			);
		}
	}
}