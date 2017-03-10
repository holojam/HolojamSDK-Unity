// ViveModuleEditor.cs
// Created by Holojam Inc. on 09.03.17

using UnityEngine;
using UnityEditor;

namespace Holojam.Vive {

  [CustomEditor(typeof(ViveModule))]
  public class ViveModuleEditor : Editor {

    SerializedProperty cameraRig;

    void OnEnable() {
      cameraRig = serializedObject.FindProperty("cameraRig");
    }

    public override void OnInspectorGUI() {
      serializedObject.Update();

      EditorGUILayout.PropertyField(cameraRig);

      ViveModule module = (ViveModule)serializedObject.targetObject;
      if (GUILayout.Button("Configure"))
        module.Configure();

      serializedObject.ApplyModifiedProperties();
    }
  }
}
