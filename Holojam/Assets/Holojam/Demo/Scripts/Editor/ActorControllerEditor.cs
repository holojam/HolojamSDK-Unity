//ActorControllerEditor.cs
//Created by Aaron C Gaudette on 05.07.16

using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ActorController)), CanEditMultipleObjects]
public class ActorControllerEditor : Holojam.Tools.ActorEditor{
   SerializedProperty head, mask;
   SerializedProperty motif, animatedEyes, skinMaterial;
   protected override void EnableDerived(){
      head = serializedObject.FindProperty("head");
      mask = serializedObject.FindProperty("mask");
      motif = serializedObject.FindProperty("motif");
      animatedEyes = serializedObject.FindProperty("animatedEyes");
      skinMaterial = serializedObject.FindProperty("skinMaterial");
   }
   protected override void DrawDerived(){
      EditorGUILayout.PropertyField(head);
      EditorGUILayout.PropertyField(mask);
      EditorGUILayout.PropertyField(motif);
      EditorGUILayout.PropertyField(animatedEyes);
      EditorGUILayout.PropertyField(skinMaterial);
   }
}
