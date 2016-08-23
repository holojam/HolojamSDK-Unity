//ActorInstantiatorEditor.cs
//Created by Aaron C Gaudette on 22.06.16

using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace Holojam.Tools{
   [CustomEditor(typeof(ActorInstantiator))]
   public class ActorInstantiatorEditor : Editor{
      SerializedProperty actor, amount;
      SerializedProperty groundPlane;
      void OnEnable(){
         actor = serializedObject.FindProperty("actor");
         amount = serializedObject.FindProperty("amount");
         groundPlane = serializedObject.FindProperty("groundPlane");
      }
      public override void OnInspectorGUI(){
         serializedObject.Update();

         GUILayout.BeginHorizontal();
            EditorGUIUtility.labelWidth = 38;
            EditorGUILayout.PropertyField(actor);
            EditorGUIUtility.labelWidth = 52;
            EditorGUILayout.PropertyField(amount);
         GUILayout.EndHorizontal();

         ActorInstantiator ai = (ActorInstantiator)serializedObject.targetObject;

         if(GUILayout.Button("Add",GUILayout.Height(20))){
            ai.Add();
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
         }
         if(GUILayout.Button("Clear All",GUILayout.Height(20))){
            ai.Clear();
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
         }

         EditorGUIUtility.labelWidth = 0;
         EditorGUILayout.PropertyField(groundPlane);

         EditorUtility.SetDirty(serializedObject.targetObject);
         EditorUtility.SetDirty(ai.GetComponent<ActorManager>());
         serializedObject.ApplyModifiedProperties();
      }
   }
}