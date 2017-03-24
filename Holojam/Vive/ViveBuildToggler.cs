// ViveBuildToggler.cs
// Created by Holojam Inc. on 01.03.17

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Holojam.Vive {

  /// <summary>
  /// Optional helper component that toggles the Vive Camera prefab and a master camera,
  /// based on whether or not the BuildManager is set for a build or master client.
  /// The editor script for this class also toggles PlayerSettings.virtualRealitySupported.
  /// </summary>
  [ExecuteInEditMode, RequireComponent(typeof(ViveModule))]
  public sealed class ViveBuildToggler : MonoBehaviour {

    public bool masterClientBuild = false;

    [SerializeField] private GameObject masterCam;
    private ViveModule module;

    void Awake() {
      module = GetComponent<ViveModule>() as ViveModule;
    }

    // Called in the editor
    void Update() {
      // Automatically set masterClientBuild if build or spectator
      if (!Tools.BuildManager.IsMasterClient()) {
        masterClientBuild = Tools.BuildManager.IsSpectator();
      }

      if (!module.cameraRig) {
        Debug.LogWarning("Holojam.Vive.ViveBuildToggler: ViveModule camera rig not assigned! ");
        return;
      }

      //if (Tools.BuildManager.IsMasterClient()) {
      if (masterClientBuild) {
        module.cameraRig.SetActive(false);
        if (masterCam) masterCam.SetActive(true);
      } else {
        module.cameraRig.SetActive(true);
        if (masterCam) masterCam.SetActive(false);
      }
    }
  }
}
