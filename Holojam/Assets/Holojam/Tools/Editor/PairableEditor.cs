//PairableEditor.cs
//Created by Aaron C Gaudette on 24.06.16

using UnityEngine;
using UnityEditor;
using Holojam.Network;

namespace Holojam.Tools{
   [CustomEditor(typeof(Pairable)), CanEditMultipleObjects]
   public class PairableEditor : Editor{
      SerializedProperty type, trackingTag, localSpace, centerOffset;
      SerializedProperty pairTime, unpairTime, cooldown;
      void OnEnable(){
         type = serializedObject.FindProperty("type");
         trackingTag = serializedObject.FindProperty("trackingTag");
         localSpace = serializedObject.FindProperty("localSpace");
         centerOffset = serializedObject.FindProperty("centerOffset");
         pairTime = serializedObject.FindProperty("pairTime");
         unpairTime = serializedObject.FindProperty("unpairTime");
         cooldown = serializedObject.FindProperty("cooldown");
      }
      public override void OnInspectorGUI(){
         serializedObject.Update();

         EditorGUILayout.PropertyField(type,new GUIContent("Pairable Type"));
         EditorGUILayout.PropertyField(trackingTag);
         EditorGUILayout.PropertyField(localSpace);
         EditorGUILayout.PropertyField(centerOffset,new GUIContent("Center Offset"));

         EditorGUILayout.Space();
         EditorGUILayout.PropertyField(pairTime);
         EditorGUILayout.PropertyField(unpairTime);
         EditorGUILayout.PropertyField(cooldown);

         if(!serializedObject.isEditingMultipleObjects){
            EditorGUILayout.Space();

            Pairable p = (Pairable)serializedObject.targetObject;
            GUIStyle style = new GUIStyle(EditorStyles.boldLabel);
            EditorGUIUtility.labelWidth=72;

            if(p.paired && !p.unpairing)
               style.normal.textColor = new Color(0.5f,1,1);

            EditorGUILayout.LabelField("Status",
               (p.paired && !p.unpairing)?"Paired with \""+p.pairedTo.pivot.gameObject.name+"\"":
               (!p.paired && !p.pairing)?"Unpaired":
               p.pairing?"Pairing...":"Unpairing...",
               style
            );
         }

         EditorUtility.SetDirty(serializedObject.targetObject);
         serializedObject.ApplyModifiedProperties();
      }
   }
}