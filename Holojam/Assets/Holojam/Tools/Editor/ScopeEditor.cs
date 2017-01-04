//ScopeEditor.cs
//Created by Aaron C Gaudette on 11.11.16

using UnityEngine;
using UnityEditor;

namespace Holojam.Tools{
   [CustomEditor(typeof(Scope))]
   public class ScopeEditor : Editor{
      SerializedProperty origin, angle;
      void OnEnable(){
         origin = serializedObject.FindProperty("origin");
         angle = serializedObject.FindProperty("angle");
      }

      public override void OnInspectorGUI(){
         serializedObject.Update();
         EditorGUILayout.PropertyField(origin);
         EditorGUILayout.PropertyField(angle);
         serializedObject.ApplyModifiedProperties();
      }
   }
}
