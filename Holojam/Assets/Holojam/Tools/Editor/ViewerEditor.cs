//ViewerEditor.cs
//Created by Aaron C Gaudette on 07.07.16

using UnityEngine;
using UnityEditor;
using Holojam.Network;

namespace Holojam{
	[CustomEditor(typeof(Viewer)), CanEditMultipleObjects]
	public class ViewerEditor : Editor{
		SerializedProperty trackingType, actor, trackingTag;
		void OnEnable(){
			trackingType=serializedObject.FindProperty("trackingType");
			actor=serializedObject.FindProperty("actor");
			trackingTag=serializedObject.FindProperty("trackingTag");
		}
		
		public override void OnInspectorGUI(){
			serializedObject.Update();
			
			trackingType.enumValueIndex=(int)(Viewer.TrackingType)
				EditorGUILayout.EnumPopup("Tracking Type",(Viewer.TrackingType)trackingType.enumValueIndex);
			
			actor.objectReferenceValue=
				EditorGUILayout.ObjectField("Actor",actor.objectReferenceValue,typeof(Actor),true);
			
			Viewer v = serializedObject.targetObject as Viewer;
			
			EditorGUI.BeginDisabledGroup(v.view==null);
				trackingTag.enumValueIndex=(int)(Motive.Tag)
					EditorGUILayout.EnumPopup("Tracking Tag",(Motive.Tag)trackingTag.enumValueIndex);
			EditorGUI.EndDisabledGroup();
			
			if(!serializedObject.isEditingMultipleObjects){
				EditorGUILayout.LabelField(
					v.actor!=null?"Tracking data is being routed through \""+
					v.actor.gameObject.name+".\" Remove reference to unlink.":
					"No actor linked. Tracking data is being sourced directly from the view"+
					(v.view!=null && v.view.Label!=""?" ("+v.view.Label+").":"."),
					new GUIStyle(EditorStyles.wordWrappedMiniLabel)
				);
			}
			
			//Make sure changes from other scripts will stick around
			EditorUtility.SetDirty(serializedObject.targetObject);
			
			serializedObject.ApplyModifiedProperties();
		}
	}
}