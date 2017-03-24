// NotifierEditor.cs
// Created by Holojam Inc. on 17.02.17

using UnityEngine;
using UnityEditor;

namespace Holojam.Network {

  [CustomEditor(typeof(Notifier))]
  public class NotifierEditor : Editor {

    public override void OnInspectorGUI() {
      EditorGUIUtility.labelWidth = 64;
      GUIStyle bold = new GUIStyle(EditorStyles.boldLabel);

      EditorGUILayout.LabelField("Events",bold);
      EditorStyles.label.wordWrap = true;

      Notifier notifier = serializedObject.targetObject as Notifier;

      foreach (string e in notifier.editorEventData)
        EditorGUILayout.LabelField("   " + e);

      if (!Application.isPlaying)
        EditorGUILayout.LabelField("   (Paused)");
      else if (notifier.editorEventData.Count == 0)
        EditorGUILayout.LabelField("   (None)");
    }
  }
}
