//BuildManagerEditor.cs
//Created by Aaron C Gaudette on 11.11.16

using UnityEngine;
using UnityEditor;

namespace Holojam.Tools{
   [CustomEditor(typeof(BuildManager))]
   public class BuildManagerEditor : Editor{
      SerializedProperty viewer;
      SerializedProperty preview, previewIndex, spectator, runtimeIndexing;
      void OnEnable(){
         viewer = serializedObject.FindProperty("viewer");
         preview = serializedObject.FindProperty("preview");
         previewIndex = serializedObject.FindProperty("previewIndex");
         spectator = serializedObject.FindProperty("spectator");
         runtimeIndexing = serializedObject.FindProperty("runtimeIndexing");
      }
      public override void OnInspectorGUI(){
         serializedObject.Update();

         EditorGUILayout.PropertyField(viewer);
         EditorGUILayout.PropertyField(preview,new GUIContent("Preview Actor",
            "Preview Unity build state for a specific actor"
         ));

         BuildManager buildManager = (BuildManager)serializedObject.targetObject;
         if(buildManager.preview){
            EditorGUILayout.PropertyField(previewIndex,new GUIContent("Index"));
            EditorGUILayout.PropertyField(spectator,new GUIContent("Spectator",
               "Preview without affecting Master Client status in the editor"
            ));
         }
         EditorGUILayout.PropertyField(runtimeIndexing);

         GUIStyle bold = new GUIStyle(EditorStyles.boldLabel);
         string label = !buildManager.buildActor?"None":
            buildManager.buildActor.labelField;
         EditorGUILayout.LabelField("Status",
            buildManager.preview?
               ("Preview (" + label + ")") + (buildManager.spectator?
               ", Master Client":""):"Master Client",
            bold
         );

         EditorGUILayout.Space();
         EditorGUILayout.LabelField("Actors",bold);
         EditorStyles.label.wordWrap = true;
         GUIStyle style = new GUIStyle();
         foreach(Actor a in Actor.instances){
            style.normal.textColor = a.debugColor;
            EditorGUILayout.LabelField("   "+a.gameObject.name+" ["+a.brand+"] "+
               (a.isLocal?"(Local) ":"")+(a.isBuild?"(Build)":""),style
            );
         }

         serializedObject.ApplyModifiedProperties();
      }
   }
}
