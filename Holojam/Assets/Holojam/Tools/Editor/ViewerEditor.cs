//ViewerEditor.cs
//Created by Aaron C Gaudette on 07.07.16

using UnityEngine;
using UnityEditor;
using Holojam.Network;

namespace Holojam.Tools{
   [CustomEditor(typeof(Viewer)), CanEditMultipleObjects]
   public class ViewerEditor : Editor{
      SerializedProperty trackingType, converter, actor, index, localSpace;
      void OnEnable(){
         trackingType = serializedObject.FindProperty("trackingType");
         converter = serializedObject.FindProperty("converter");
         actor = serializedObject.FindProperty("actor");
         index = serializedObject.FindProperty("index");
         localSpace=serializedObject.FindProperty("localSpace");
      }

      public override void OnInspectorGUI(){
         serializedObject.Update();
         Viewer v = serializedObject.targetObject as Viewer;

         if(v.converter.device!=Converter.Device.VIVE){
            EditorGUILayout.PropertyField(trackingType);
            EditorGUILayout.PropertyField(converter);
            EditorGUILayout.PropertyField(actor);

            if(v.view!=null){
               EditorGUILayout.PropertyField(index);
               EditorGUILayout.PropertyField(localSpace);
            }

            if(!serializedObject.isEditingMultipleObjects){
               EditorGUILayout.LabelField(
                  v.actor!=null?"Tracking data is being routed through \""+
                  v.actor.gameObject.name+".\" Remove reference to unlink.":
                  "No actor linked. Tracking data is being sourced from the view"+
                  (v.view!=null && v.view.label!=""?" ("+v.view.label+").":"."),
                  new GUIStyle(EditorStyles.wordWrappedMiniLabel)
               );
            }
         }else{
            EditorGUILayout.LabelField("Viewer disabled: Device is " + v.converter.device,
               new GUIStyle(EditorStyles.wordWrappedMiniLabel));
         }

         serializedObject.ApplyModifiedProperties();
      }
   }
}
