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

    const float MIN_DELTA = .004f; //m
    const float RETRY_RATE = 4; //s

    SteamVR_Events.Action posesAction;
    Vector3[] lighthouses = new Vector3[2];

    /// <summary>
    /// Has the lighthouse data been loaded in properly?
    /// </summary>
    bool ready = false;

    /// <summary>
    /// Has a successful calibration been performed?
    /// </summary>
    bool calibrated = false;

    ViveModule module;
    float lastTime = 0;

    Vector3 cachedPosition = Vector3.zero;
    Quaternion cachedRotation = Quaternion.identity;

    Vector3 calibratedPosition = Vector3.zero;
    Quaternion calibratedRotation = Quaternion.identity;

    /// <summary>
    /// If calibration is possible, do calibration.
    /// <returns>True if successful, false if there was an error.</returns>
    /// </summary>
    public bool Calibrate() {
      if (!module.cameraRig) {
        Network.RemoteLogger.Log(
          "Calibration failed; ViveModule camera rig not assigned"
        );
        return false;
      }

      if (!ready) {
        Network.RemoteLogger.Log(
          "Calibration failed; lighthouses not found"
        );
        return false;
      }

      // Relative difference, not absolute
      if (Mathf.Abs(lighthouses[0].y - lighthouses[1].y) < MIN_DELTA) {
        Network.RemoteLogger.Log(
          "Calibration failed; lighthouse heights are too similar (within "
          + (100 * MIN_DELTA) + " cm)"
        );
        return false;
      }

      return DoCalibration(module.cameraRig.transform, lighthouses[0], lighthouses[1]);
    }

    /// <summary>
    /// Initializes, and caches the position/rotation of the centroid.
    /// </summary>
    void Awake() {
      posesAction = SteamVR_Events.NewPosesAction(OnNewPoses);
      module = GetComponent<ViveModule>() as ViveModule;

      if (!module.cameraRig) {
        Debug.Log("Holojam.Vive.ViveCalibrator: ViveModule camera rig not assigned!");
        return;
      }

      cachedPosition = module.cameraRig.transform.localPosition;
      cachedRotation = module.cameraRig.transform.localRotation;
    }

    /// <summary>
    /// Triggers a calibration as soon as the lighthouses become tracked,
    /// or regularly at an interval of RETRY_RATE seconds.
    /// Fixes the centroid in place after a successful calibration.
    /// </summary>
    void Update() {
      if (!calibrated && (ready || Time.time > lastTime + RETRY_RATE)) {
        calibrated = Calibrate();
        lastTime = Time.time;
      }

      else if (calibrated) {
        module.cameraRig.transform.localPosition = calibratedPosition;
        module.cameraRig.transform.localRotation = calibratedRotation;
        module.cameraRig.transform.localScale = Vector3.one;
      }
    }

    /// <summary>
    /// Performs a calibration, given a centroid and two lighthouse positions.
    /// </summary>
    bool DoCalibration(Transform centroid, Vector3 l0, Vector3 l1) {
      if (l1.y > l0.y) { // Highest lighthouse is always first
        Vector3 swap = l0;
        l0 = l1;
        l1 = swap;
      }

      // Zero the height (the rest is 2D)
      l0 = new Vector3(l0.x, 0, l0.z);
      l1 = new Vector3(l1.x, 0, l1.z);

      // Calculate the forward vector with the diagonal
      Vector3 forward = l1 - l0;
      Quaternion rotation = Quaternion.Inverse(
        Quaternion.LookRotation(forward)
      );
      centroid.localRotation = cachedRotation * rotation;

      // Offset the centroid by its difference to the tracking center,
      // relative to the forward vector
      Vector3 offset = .5f * (l0 + l1);
      centroid.localPosition = cachedPosition + rotation * -offset;

      forward.Normalize(); // For debugging

      Network.RemoteLogger.Log(
        "Calibration successful: offset = (" + offset.x + ", " + offset.z
        + "), forward = (" + forward.x + ", " + forward.z + ")"
      );

      calibratedPosition = centroid.localPosition;
      calibratedRotation = centroid.localRotation;

      return true;
    }

    /// <summary>
    /// Get the lighthouse positions from SteamVR.
    /// </summary>
    void OnNewPoses(TrackedDevicePose_t[] poses) {
      ready = false;

      // Need at least three slots
      if (poses.Length < 3) return;

      var system = OpenVR.System;
      if (system == null) return;

      int index = 0;
      for (int i = 0; i < poses.Length && index < 2; ++i) {
        if (!poses[i].bDeviceIsConnected || !poses[i].bPoseIsValid)
          continue;

        // Filter lighthouses
        if (system.GetTrackedDeviceClass((uint)i) == ETrackedDeviceClass.TrackingReference) {
          lighthouses[index++] =
            new SteamVR_Utils.RigidTransform(poses[i].mDeviceToAbsoluteTracking).pos;
        }
      }

      ready = index == 2;
    }

    void OnEnable() {
      posesAction.enabled = true;
    }

    void OnDisable() {
      posesAction.enabled = false;
    }
  }
}
