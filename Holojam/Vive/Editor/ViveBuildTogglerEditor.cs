// ViveBuildTogglerEditor.cs
// Created by Holojam Inc. on 01.03.17

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Holojam.Vive {

  [CustomEditor(typeof(ViveBuildToggler))]
  public class ViveBuildTogglerEditor : Editor {

    SerializedProperty masterClientBuild, masterCam;

    void OnEnable() {
      masterClientBuild = serializedObject.FindProperty("masterClientBuild");
      masterCam = serializedObject.FindProperty("masterCam");
    }

    public override void OnInspectorGUI() {
      serializedObject.Update();

      GUI.enabled = Tools.BuildManager.IsMasterClient() && !Tools.BuildManager.IsSpectator();
      EditorGUILayout.PropertyField(masterClientBuild);
      GUI.enabled = true;

      EditorGUILayout.PropertyField(masterCam);

      PlayerSettings.virtualRealitySupported =
        !((ViveBuildToggler)serializedObject.targetObject).masterClientBuild;

      EditorGUILayout.LabelField("Virtual Reality Supported: " + PlayerSettings.virtualRealitySupported);

      serializedObject.ApplyModifiedProperties();
    }
  }
}

