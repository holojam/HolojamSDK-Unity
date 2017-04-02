// HolojamUnity.cs
// Created by Holojam Inc. on 05.03.17

using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Holojam.Utility {

  /// <summary>
  /// Utility class for the Holojam prefab.
  /// </summary>
  public class HolojamUnity : MonoBehaviour {

    #if UNITY_EDITOR
    void UpdateController(Network.Controller controller) {
      EditorUtility.SetDirty((UnityEngine.Object)controller);
    }
    #endif
  }
}
