// ViveCalibratorEditor.cs
// Created by Holojam Inc. on 03.03.17

using UnityEditor;

namespace Holojam.Vive {
  [CustomEditor(typeof(ViveCalibrator))]
  public class ViveCalibratorEditor : Editor {

    SerializedProperty view, centroid, canCalibrate;

    void OnEnable() {
      view = serializedObject.FindProperty("view");
      centroid = serializedObject.FindProperty("centroid");
      canCalibrate = serializedObject.FindProperty("canCalibrate");
    }

    public override void OnInspectorGUI() {
      serializedObject.Update();

      EditorGUILayout.PropertyField(view);
      EditorGUILayout.PropertyField(centroid);

      if (Tools.BuildManager.IsMasterClient())
        EditorGUILayout.PropertyField(canCalibrate);

      serializedObject.ApplyModifiedProperties();
    }
  }
}
