//ViewerEditor.cs
//Created by Aaron C Gaudette on 07.07.16

using UnityEngine;
using UnityEditor;
using Holojam.Network;

namespace Holojam.Tools{
   [CustomEditor(typeof(Viewer)), CanEditMultipleObjects]
   public class ViewerEditor : Editor{
      SerializedProperty trackingType, actor, trackingTag, localSpace;
      void OnEnable(){
         trackingType=serializedObject.FindProperty("trackingType");
         actor=serializedObject.FindProperty("actor");
         trackingTag=serializedObject.FindProperty("trackingTag");
         localSpace=serializedObject.FindProperty("localSpace");
      }

      public override void OnInspectorGUI(){
         serializedObject.Update();

         EditorGUILayout.PropertyField(trackingType);
         EditorGUILayout.PropertyField(actor);

         Viewer v = serializedObject.targetObject as Viewer;

         EditorGUI.BeginDisabledGroup(v.view==null);
            EditorGUILayout.PropertyField(trackingTag);
            EditorGUILayout.PropertyField(localSpace);
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

         //EditorUtility.SetDirty(serializedObject.targetObject);
         serializedObject.ApplyModifiedProperties();
      }
   }
}