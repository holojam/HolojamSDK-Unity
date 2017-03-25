// ViveCalibrator.cs
// Created by Holojam Inc. on 01.03.17

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

namespace Holojam.Vive {

  /// <summary>
  /// "Calibrates" a Vive space (a room that has been delineated independently, instead of
  /// a single integrated tracking system) automatically, offsetting its centroid to fit
  /// within the physical world and standardizing the forward direction.
  /// </summary>
  [RequireComponent(typeof(ViveModule))]
  public sealed class ViveCalibrator : MonoBehaviour {

    const float ERROR = .04f; // 4cm

    /// <summary>
    /// If not enabled, the component will not attempt to calibrate.
    /// </summary>
    public bool enable = true;

    SteamVR_Events.Action posesAction;
    Vector3[] lighthouses = new Vector3[2];
    bool ready = false;

    ViveModule module;
    Vector3 cachedPosition;

    /// <summary>
    /// 
    /// </summary>
    public bool Calibrate() {
      if (!Valid())
        return false;

      if (!ready) {
        Network.RemoteLogger.Log(
          "Holojam.Vive.ViveCalibrator: Calibration failed, lighthouses not found"
        );
        return false;
      }

      if (lighthouses[0].y - lighthouses[1].y < ERROR) { // Relative difference, not absolute
        Network.RemoteLogger.Log(
          "Holojam.Vive.ViveCalibrator: Lighthouse heights are too similar to calibrate."
        );
        return false;
      }

      if (!module.cameraRig) {
        Network.RemoteLogger.Log(
          "Holojam.Vive.ViveCalibrator: Calibration failed, ViveModule camera rig not assigned!"
        );
        return false;
      }

      return DoCalibration(lighthouses[0], lighthouses[1]);
    }

    bool Valid() {
      return !Tools.BuildManager.IsMasterClient() && !Tools.BuildManager.IsSpectator() && enable;
    }

    void Awake() {
      posesAction = SteamVR_Events.NewPosesAction(OnNewPoses);
      module = GetComponent<ViveModule>() as ViveModule;

      if (!Valid()) return;

      if (!module.cameraRig) {
        Debug.Log(
          "Holojam.Vive.ViveCalibrator: Calibration failed, ViveModule camera rig not assigned!"
        );
        return;
      }
      cachedPosition = module.cameraRig.transform.localPosition;

      StartCoroutine(WaitToCalibrate());
    }

    IEnumerator WaitToCalibrate() {
      // Wait for the lighthouses to become tracked
      while (!ready) yield return null;
      Calibrate();
    }

    bool DoCalibration(Vector3 l0, Vector3 l1) {
      if (l1.y > l0.y) { // Highest lighthouse is first
        Vector3 swap = l0;
        l0 = l1;
        l1 = swap;
      }

      l0 = new Vector3(l0.x, 0, l0.z);
      l1 = new Vector3(l1.x, 0, l1.z);

      // Offset the centroid by its difference to the tracking center
      Vector3 offset = cachedPosition - .5f * (l0 + l1);
      module.cameraRig.transform.localPosition = offset;

      // Calculate the forward vector with the corner
      Vector3 forward = l0 - new Vector3(l0.x, 0, l1.z);
      module.cameraRig.transform.localRotation = Quaternion.LookRotation(forward);

      Debug.Log(
        "Holojam.Vive.Calibrator: Calibration successful--Offset = (" + offset.x + ", " + offset.z
        + "), Forward = (" + forward.x + ", " + forward.z + ")"
      );

      return true;
    }

    /// <summary>
    /// Get the lighthouse positions.
    /// </summary>
    void OnNewPoses(TrackedDevicePose_t[] poses) {
      ready = false;

      // Return if both lighthouses aren't tracked
      if (poses.Length < 3) return;

      for (int i = 1; i <= 2; ++i) {
        if (!poses[i].bDeviceIsConnected || !poses[i].bPoseIsValid)
          return;

        var pose = new SteamVR_Utils.RigidTransform(poses[i].mDeviceToAbsoluteTracking);
        lighthouses[i-1] = pose.pos;
      }

      ready = true;
    }

    void OnEnable() {
      posesAction.enabled = true;
    }

    void OnDisable() {
      posesAction.enabled = false;
    }
  }
}
