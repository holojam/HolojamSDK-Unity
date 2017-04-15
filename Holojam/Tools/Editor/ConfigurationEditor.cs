// ConfigurationEditor.cs
// Created by Holojam Inc. on 13.04.17

using UnityEngine;
using UnityEditor;

namespace Holojam.Tools {

  [CustomEditor(typeof(Configuration))]
  public class ConfigurationEditor : Editor {

    const int ORDER = -16001;

    SerializedProperty path;

    void OnEnable() {
      if (!Application.isPlaying) {
        MonoScript config = MonoScript.FromMonoBehaviour(
          (Configuration)serializedObject.targetObject
        );

        if (MonoImporter.GetExecutionOrder(config) != ORDER) {
          MonoImporter.SetExecutionOrder(config, ORDER);
          Debug.Log(
            "Holojam.Tools.Configuration: Initialized execution order (" + ORDER + ")",
            serializedObject.targetObject
          );
        }
      }

      path = serializedObject.FindProperty("configFilePath");
    }

    public override void OnInspectorGUI() {
      serializedObject.Update();

      EditorGUILayout.PropertyField(path);

      serializedObject.ApplyModifiedProperties();
    }
  }
}
