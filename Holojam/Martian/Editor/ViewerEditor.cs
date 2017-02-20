//ViewerEditor.cs
//Created by Aaron C Gaudette on 07.07.16

using UnityEngine;
using UnityEditor;
using Holojam.Network;

namespace Holojam.Martian{
   [CustomEditor(typeof(Viewer)), CanEditMultipleObjects]
   public class ViewerEditor : Editor{
      SerializedProperty trackingType, converter, localSpace;
      void OnEnable(){
         trackingType = serializedObject.FindProperty("trackingType");
         converter = serializedObject.FindProperty("converter");
      }

      public override void OnInspectorGUI(){
         serializedObject.Update();
         Viewer v = serializedObject.targetObject as Viewer;

         if(Tools.BuildManager.DEVICE!=Tools.BuildManager.Device.VIVE){
            EditorGUILayout.PropertyField(trackingType);
            EditorGUILayout.PropertyField(converter);

            if(!serializedObject.isEditingMultipleObjects){
               EditorGUILayout.LabelField(
                  v.actor!=null?"Tracking data is being routed through \""+
                  v.actor.gameObject.name+"\".":"No actor linked.",
                  new GUIStyle(EditorStyles.wordWrappedMiniLabel)
               );
            }
         }else{
            EditorGUILayout.LabelField("Viewer disabled: Device is " + Tools.BuildManager.DEVICE,
               new GUIStyle(EditorStyles.wordWrappedMiniLabel));
         }

         serializedObject.ApplyModifiedProperties();
      }
   }
}
