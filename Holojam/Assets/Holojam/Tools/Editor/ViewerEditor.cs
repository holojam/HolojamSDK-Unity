//ViewerEditor.cs
//Created by Aaron C Gaudette on 07.07.16

using UnityEngine;
using UnityEditor;

namespace Holojam{
	[CustomEditor(typeof(Viewer)), CanEditMultipleObjects]
	public class ViewerEditor : Editor{
		SerializedProperty trackingType, actor;
		void OnEnable(){
			trackingType=serializedObject.FindProperty("trackingType");
			actor=serializedObject.FindProperty("actor");
		}
		
		public override void OnInspectorGUI(){
			serializedObject.Update();
			
			trackingType.enumValueIndex=(int)(Viewer.TrackingType)
				EditorGUILayout.EnumPopup("Tracking Type",(Viewer.TrackingType)trackingType.enumValueIndex);
			
			actor.objectReferenceValue=
				EditorGUILayout.ObjectField("Actor",actor.objectReferenceValue,typeof(Actor),true);
			
			if(!serializedObject.isEditingMultipleObjects){
				Viewer v = serializedObject.targetObject as Viewer;
				EditorStyles.label.wordWrap = true;
				EditorGUILayout.LabelField(
					v.actor!=null?"Tracking data is being routed through "+
					v.actor.handle+" ("+(v.actor.index+1)+"). Remove reference to unlink.":
					"No actor linked. Tracking data is being sourced directly from the view"+
					(v.view!=null && v.view.label!=""?" ("+v.view.label+").":".")
				);
			}
			
			//Make sure changes from other scripts will stick around
			EditorUtility.SetDirty(serializedObject.targetObject);
			
			serializedObject.ApplyModifiedProperties();
		}
	}
}