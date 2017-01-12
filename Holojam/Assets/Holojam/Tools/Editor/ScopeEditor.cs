//ScopeEditor.cs
//Created by Aaron C Gaudette on 11.11.16

using UnityEngine;
using UnityEditor;

namespace Holojam.Tools{
   [CustomEditor(typeof(Scope))]
   public class ScopeEditor : Editor{
      SerializedProperty origin, height, angle;
      void OnEnable(){
         origin = serializedObject.FindProperty("origin");
         angle = serializedObject.FindProperty("angle");
         height = serializedObject.FindProperty("height");
      }

      public override void OnInspectorGUI(){
         serializedObject.Update();
         EditorGUILayout.PropertyField(origin);
         EditorGUILayout.PropertyField(height);
         EditorGUILayout.PropertyField(angle);
         serializedObject.ApplyModifiedProperties();
      }
   }
}
