﻿// ViveBuildToggler.cs
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
  [ExecuteInEditMode]
  public class ViveBuildToggler : MonoBehaviour {

    public bool masterClientBuild = false;

    [SerializeField] private GameObject cameraRig;
    [SerializeField] private GameObject masterCam;

    // Called in the editor
    void Update() {
      if (!cameraRig || !masterCam)
        return;

      //if (Tools.BuildManager.IsMasterClient()) {
      if (masterClientBuild) {
        cameraRig.SetActive(false);
        if (masterCam) masterCam.SetActive(true);
      } else {
        cameraRig.SetActive(true);
        if (masterCam) masterCam.SetActive(false);
      }
    }
  }
}