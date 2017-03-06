// ActorEditor.cs
// Created by Aaron C Gaudette on 05.07.16

using UnityEngine;
using UnityEditor;

namespace Holojam.Tools{

  [CustomEditor(typeof(Actor)), CanEditMultipleObjects]
  public class ActorEditor : Editor {

    // Override these functions if you desire a custom inspector
    protected virtual void EnableDerived() { }
    protected virtual void DrawDerived() { }

    SerializedProperty view, smooth, scope, index;
    void OnEnable() {
      view = serializedObject.FindProperty("view");
      smooth = serializedObject.FindProperty("smooth");
      scope = serializedObject.FindProperty("scope");
      index = serializedObject.FindProperty("index");

      EnableDerived();
    }

    public override void OnInspectorGUI() {
      serializedObject.Update();

      EditorGUILayout.PropertyField(view);

      EditorGUIUtility.labelWidth = 64;
      EditorGUILayout.BeginHorizontal();
        EditorGUILayout.PropertyField(scope);
        EditorGUILayout.PropertyField(index);
      EditorGUILayout.EndHorizontal();

      EditorGUILayout.PropertyField(smooth);

      EditorGUIUtility.labelWidth = 0;
      DrawDerived();

      if (!serializedObject.isEditingMultipleObjects) {
        Actor a = serializedObject.targetObject as Actor;
        if (a==null) {
          serializedObject.ApplyModifiedProperties();
          return;
        }

        GUIStyle style = new GUIStyle(EditorStyles.boldLabel);
        if (Application.isPlaying)
          style.normal.textColor = a.IsLocal ?
            new Color(0.5f, 1, 0.5f) : new Color(1, 0.5f, 0.5f);

        EditorGUILayout.LabelField(
          "Status", (a.IsLocal ? "Local" : "Not Local"), style
        );

        /*
        if (!a.actorManager.runtimeIndexing && Application.isPlaying) {
           EditorGUILayout.LabelField(
            "Runtime indexing is OFF. "
            + "Actor will not reflect changes under manager during playmode.",
            new GUIStyle(EditorStyles.wordWrappedMiniLabel)
          );
        }
        */
      }

      serializedObject.ApplyModifiedProperties();
    }
  }
}
