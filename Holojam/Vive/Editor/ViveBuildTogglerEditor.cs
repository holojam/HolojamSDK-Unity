using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Holojam.Vive {
  [CustomEditor(typeof(ViveBuildToggler))]
  public class ViveBuildTogglerEditor : Editor {

    public override void OnInspectorGUI() {
      base.OnInspectorGUI();

      PlayerSettings.virtualRealitySupported = !Tools.BuildManager.IsMasterClient();

      EditorGUILayout.LabelField("Virtual Reality Supported: " + PlayerSettings.virtualRealitySupported);
    }
  }
}

