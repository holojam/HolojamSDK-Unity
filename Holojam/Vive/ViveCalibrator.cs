// ViveCalibrator.cs
// Created by Holojam Inc. on 01.03.17

using System;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

namespace Holojam.Vive {

  /// <summary>
  /// "Calibrates" a Relay space (a room that has been delineated independently, instead of
  /// a single integrated tracking system) by comparing its tracking centroid to an absolute center.
  /// Synchronizes a flag (canCalibrate) to control when clients can respond to a calibration
  /// signal.
  /// </summary>
  [RequireComponent(typeof(ViveControllerReceiver))]
  public class ViveCalibrator : Holojam.Tools.Synchronizable {

    /// <summary>
    /// A transform representing the tracking bounds, e.g. the CameraRig prefab.
    /// </summary>
    public Transform centroid;

    private ViveControllerReceiver receiver;
    private Vector3 cachedPosition;

    /// <summary>
    /// If true, a global space calibration will be triggered on button press.
    /// Set this to true at runtime to allow calibration for all the clients, then
    /// unset it to disable accidental calibrations. We recommend mapping it to a
    /// button press or UI element.
    /// </summary>
    [SerializeField] private bool canCalibrate;

    /// <summary>
    /// Public setter for canCalibrate.
    /// </summary>
    public void SetCanCalibrate(bool canCalibrate) {
      this.canCalibrate = canCalibrate;
    }

    public override string Label { get { return "RelayCalibrator"; } }
    public override bool Host { get { return false; } }
    public override bool AutoHost { get { return true; } }

    protected override void Awake() {
      base.Awake();

      receiver = GetComponent<ViveControllerReceiver>();
      cachedPosition = centroid.position;
    }

    /// <summary>
    /// If the master client has allowed calibration, check every frame for a "Grip Press Down"
    /// event from the attached controller receiver.
    /// </summary>
    protected override void Update() {
      base.Update();

      receiver.label = Network.Canon.IndexToLabel(Tools.BuildManager.BUILD_INDEX, "right");
      if (receiver.GetPressDown(EVRButtonId.k_EButton_Grip) && canCalibrate)
        // Calibrate to the relative position of the controller
        Calibrate(receiver.TrackedPosition - centroid.position);
    }

    /// <summary>
    /// The only synchronized value is an int, acting as a boolean.
    /// </summary>
    public override void ResetData() {
      data = new Network.Flake(0, 0, 0, 1);
    }

    /// <summary>
    /// Synchronize the calibration permission flag.
    /// </summary>
    protected override void Sync() {
      if (Sending) {
        data.ints[0] = (canCalibrate ? 1 : 0);
      } else {
        canCalibrate = (data.ints[0] == 1);
      }
    }

    /// <summary>
    /// Offset the centroid by its difference to the absolute center.
    /// </summary>
    void Calibrate(Vector3 center) {
      if (Tools.BuildManager.IsMasterClient()) return;
      centroid.position = cachedPosition - new Vector3(center.x, 0, center.z);
    }
  }
}
