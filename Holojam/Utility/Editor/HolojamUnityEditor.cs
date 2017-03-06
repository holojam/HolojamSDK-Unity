// HolojamUnityEditor.cs
// Created by Holojam Inc. on 05.03.17

using UnityEditor;

namespace Holojam.Utility {

  [CustomEditor(typeof(HolojamUnity)), CanEditMultipleObjects]
  public class HolojamUnityEditor : Editor {

    public override void OnInspectorGUI() { }
  }
}
