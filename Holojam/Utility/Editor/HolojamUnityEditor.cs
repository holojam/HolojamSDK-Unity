// HolojamUnityEditor.cs
// Created by Holojam Inc. on 05.03.17

using System.Reflection;
using UnityEditor;
using Holojam.Tools;

namespace Holojam.Utility {

  [CustomEditor(typeof(HolojamUnity)), CanEditMultipleObjects]
  public class HolojamUnityEditor : Editor {

    public override void OnInspectorGUI() {
      serializedObject.Update();

      string holojamData = "Not loaded";
      System.Version v = typeof(Configuration).Assembly.GetName().Version;
      if (v.Major > 0 || v.Minor > 0 || v.Revision > 0)
        holojamData = "v" + v;

      EditorGUILayout.LabelField("Holojam", holojamData);
      EditorGUILayout.LabelField("Core", CoreInfo.GetVersion());
    }
  }
}
