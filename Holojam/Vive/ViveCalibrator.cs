// ViveCalibrator.cs
// Created by Holojam Inc. on 01.03.17

using System;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

namespace Holojam.Vive {
  [RequireComponent(typeof(ViveControllerReceiver))]
  public class ViveCalibrator : Holojam.Tools.Synchronizable {

    public GameObject centroid;

    private ViveControllerReceiver receiver;

    /// <summary>
    /// Toggle for whether or not clients can calibrate. Used on the master client.
    /// </summary>
    private bool calibrate = false;

    /// <summary>
    /// Specific label for calibration.
    /// </summary>
    public override string Label { get { return "vive-calibration"; } }

    public override bool Host { get { return false; } }

    public override bool AutoHost { get { return true; } }

    /// <summary>
    /// Use a specific controller as the calibrator.
    /// </summary>
    protected override void Awake() {
      base.Awake();
      receiver = GetComponent<ViveControllerReceiver>();
    }

    /// <summary>
    /// If the master client has okayed calibration, check every frame for a "Grip Press Down" event from a
    /// named controller.
    /// </summary>
    protected override void Update() {
      base.Update();
      if (receiver.GetPressDown(EVRButtonId.k_EButton_Grip) && calibrate) {
        centroid.transform.position = receiver.TrackedPosition;
        //Don't recenter the client that is providing the center.
        //if (!(receiver.label.Contains(Network.Canon.IndexToLabel(Tools.BuildManager.BUILD_INDEX)))) {
        //  centroid.transform.position = receiver.TrackedPosition;
        //}
      }
    }

    /// <summary>
    /// Only synchronized value is one int, acting as a boolean.
    /// </summary>
    public override void ResetData() {
      data = new Network.Flake(0, 0, 0, 1);
    }

    protected override void Sync() {
      if (Sending) {
        data.ints[0] = (calibrate ? 1 : 0);
      } else {
        calibrate = (data.ints[0] == 1);
      }
    }

    public void SetCalibrate(bool c) {
      calibrate = c;
    }
  }
}

