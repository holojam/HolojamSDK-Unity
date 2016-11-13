//TrackableEditor.cs
//Created by Aaron C Gaudette on 11.11.16

using UnityEngine;
using UnityEditor;

namespace Holojam.Tools{
   [CustomEditor(typeof(Trackable)),CanEditMultipleObjects]
   public class TrackableEditor : Editor{
      SerializedProperty label, scope, localSpace;
      void OnEnable(){
         label = serializedObject.FindProperty("label");
         scope = serializedObject.FindProperty("scope");
         localSpace = serializedObject.FindProperty("localSpace");
      }

      public override void OnInspectorGUI(){
         serializedObject.Update();
         EditorGUILayout.PropertyField(label);
         EditorGUILayout.PropertyField(scope);
         EditorGUILayout.PropertyField(localSpace);
         serializedObject.ApplyModifiedProperties();
      }
   }
}
