// ViveCalibrator.cs
// Created by Holojam Inc. on 01.03.17

using System;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

namespace Holojam.Vive {

  /// <summary>
  /// 
  /// </summary>
  public class ViveCalibrator : Holojam.Tools.Synchronizable {

    /// <summary>
    /// 
    /// </summary>
    public Transform centroid;

    private ViveControllerReceiver receiver;

    /// <summary>
    /// If true, a global space calibration will be triggered on button press.
    /// </summary>
    [SerializeField] private bool canCalibrate;

    public override string Label { get { return "RelayCalibrator"; } }
    public override bool Host { get { return false; } }
    public override bool AutoHost { get { return true; } }

    protected override void Awake() {
      base.Awake();
      receiver = GetComponent<ViveControllerReceiver>();
    }

    /// <summary>
    /// If the master client has allowed calibration, check every frame for a "Grip Press Down"
    /// event from the attached controller receiver.
    /// </summary>
    protected override void Update() {
      base.Update();

      if (receiver.GetPressDown(EVRButtonId.k_EButton_Grip) && canCalibrate)
        Calibrate(receiver.TrackedPosition);
    }

    /// <summary>
    /// The only synchronized value is an int, acting as a boolean.
    /// </summary>
    public override void ResetData() {
      data = new Network.Flake(0, 0, 0, 1);
    }

    /// <summary>
    /// 
    /// </summary>
    protected override void Sync() {
      if (Sending) {
        data.ints[0] = (canCalibrate ? 1 : 0);
      } else {
        canCalibrate = (data.ints[0] == 1);
      }
    }

    /// <summary>
    /// 
    /// </summary>
    void Calibrate(Vector3 center) {
      if (Tools.BuildManager.IsMasterClient()) return;

      Vector3 difference = centroid.position - center;
      difference = new Vector3(difference.x, 0, difference.z);
      centroid.Translate(difference);

      Debug.Log(
        "Holojam.Vive.ViveCalibrator: space calibrated with offset ("
        + difference.x + ", " + difference.z + ")"
      );
    }
  }
}

