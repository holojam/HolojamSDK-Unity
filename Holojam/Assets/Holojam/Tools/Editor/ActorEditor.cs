//ActorEditor.cs
//Created by Aaron C Gaudette on 05.07.16

using UnityEngine;
using UnityEditor;

namespace Holojam.Tools{
   [CustomEditor(typeof(Actor)), CanEditMultipleObjects]
   public class ActorEditor : Editor{
      //Override these functions if you desire a custom inspector
      protected virtual void EnableDerived(){}
      protected virtual void DrawDerived(){}

      SerializedProperty scope, index, localSpace;
      void OnEnable(){
         scope = serializedObject.FindProperty("scope");
         index = serializedObject.FindProperty("index");
         localSpace = serializedObject.FindProperty("localSpace");

         EnableDerived();
      }
      public override void OnInspectorGUI(){
         serializedObject.Update();

         EditorGUIUtility.labelWidth = 64;
         EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(scope);
            EditorGUILayout.PropertyField(index);
         EditorGUILayout.EndHorizontal();

         EditorGUIUtility.labelWidth = 0;
         EditorGUILayout.PropertyField(localSpace);

         DrawDerived();

         if(!serializedObject.isEditingMultipleObjects){
            Actor a = serializedObject.targetObject as Actor;
            if(a==null){
               serializedObject.ApplyModifiedProperties();
               return;
            }

            GUIStyle style = new GUIStyle(EditorStyles.boldLabel);
            if(Application.isPlaying)
               style.normal.textColor = a.isLocal?
                  new Color(0.5f,1,0.5f):new Color(1,0.5f,0.5f);

            EditorGUILayout.LabelField("Status",
               (a.isLocal?"Local":"Not Local"),
               style
            );

            /*
            if(!a.actorManager.runtimeIndexing && Application.isPlaying){
               EditorGUILayout.LabelField(
                  "Runtime indexing is OFF. "+
                  "Actor will not reflect changes under manager during playmode.",
                  new GUIStyle(EditorStyles.wordWrappedMiniLabel)
               );
            }
            */
         }

         serializedObject.ApplyModifiedProperties();
      }
   }
}
