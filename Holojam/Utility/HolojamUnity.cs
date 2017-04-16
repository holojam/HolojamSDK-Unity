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

    Network.Client client;

    #if UNITY_EDITOR
    void UpdateController(Network.Controller controller) {
      EditorUtility.SetDirty((UnityEngine.Object)controller);
    }
    #endif

    void Start() {
      client = GetComponent<Network.Client>();
    }

    void Update() {
      Tools.InfoPanel.SetString(
        "build-index", "Build Index: " + Tools.BuildManager.BUILD_INDEX
        + " (" + (Tools.BuildManager.IsMasterClient() ? "Master Client" :
          Tools.BuildManager.IsSpectator() ? "Spectator" : "Client")
        + ")"
      );

      Tools.InfoPanel.SetString(
        "ip", "Relay: " + client.RelayAddress
          + ":" + client.UpstreamPort
      );
    }
  }
}
