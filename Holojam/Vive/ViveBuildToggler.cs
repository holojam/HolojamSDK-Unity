using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Holojam.Vive {
  /// <summary>
  /// Helper class that automatically toggles the Vive Camera prefab and a master camera,
  /// based on whether or not the BuildManager is set for a build or master client.
  /// The editor script for this class also toggles PlayerSettings.virtualRealitySupported.
  /// </summary>
  [ExecuteInEditMode]
  public class ViveBuildToggler : MonoBehaviour {

    [SerializeField] private GameObject cameraRig;
    [SerializeField] private GameObject masterCam;

    // Update is called once per frame
    void Update() {
      if (!cameraRig || !masterCam)
        return;

      if (Tools.BuildManager.IsMasterClient()) {
        masterCam.SetActive(true);
        cameraRig.SetActive(false);
      } else {
        masterCam.SetActive(false);
        cameraRig.SetActive(true);
      }
    }
  }
}

