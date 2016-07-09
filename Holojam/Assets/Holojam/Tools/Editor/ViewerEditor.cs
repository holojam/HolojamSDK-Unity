//ViewerEditor.cs
//Created by Aaron C Gaudette on 07.07.16

using UnityEngine;
using UnityEditor;

namespace Holojam{
	[CustomEditor(typeof(Viewer))]
	public class ViewerEditor : Editor{
		public override void OnInspectorGUI(){
			Viewer v = (Viewer)target;
			
			v.trackingType=(Viewer.TrackingType)EditorGUILayout.EnumPopup("Tracking Type",v.trackingType);
			
			v.actor=EditorGUILayout.ObjectField("Actor",v.actor,typeof(Actor),true) as Actor;
			
			EditorStyles.label.wordWrap = true;
			EditorGUILayout.LabelField(
				v.actor!=null?"Tracking data is being routed through "+
				v.actor.name+" ("+(v.actor.index+1)+"). Remove reference to unlink.":
				"No actor linked. Tracking data is being sourced directly from the view"+
				(v.view!=null && v.view.label!=""?" ("+v.view.label+").":".")
			);
			
			EditorUtility.SetDirty(v); //Make sure changes from other scripts will stick around
		}
	}
}