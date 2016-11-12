//ActorControllerEditor.cs
//Created by Aaron C Gaudette on 05.07.16

using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ActorController)), CanEditMultipleObjects]
public class ActorControllerEditor : Holojam.Tools.ActorEditor{
   SerializedProperty head, animatedEyes;
   protected override void EnableDerived(){
      head = serializedObject.FindProperty("head");
      animatedEyes = serializedObject.FindProperty("animatedEyes");
   }
   protected override void DrawDerived(){
      EditorGUILayout.PropertyField(head);
      EditorGUILayout.PropertyField(animatedEyes);
   }
}