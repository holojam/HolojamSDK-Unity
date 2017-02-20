// Remote.cs
// Created by Holojam Inc. on 03.01.17

using UnityEngine;

namespace Holojam.Martian {

  /// <summary>
  /// Two-way Controller for sending and receiving data from the Holojam remote app.
  /// </summary>
  public class Remote : Network.Controller {

    /// <summary>
    /// Device selector, since this won't be in a scene with a BuildManager.
    /// </summary>
    public Tools.BuildManager.Device device = Tools.BuildManager.DEVICE_DEFAULT;

    protected override ProcessDelegate Process { get { return UpdateRemote; } }

    /// <summary>"Holoscope"</summary>
    public override string Scope { get { return "Holoscope"; } }

    /// <summary>"Remote"</summary>
    public override string Label { get { return "Remote"; } }

    /// <summary>
    /// Only send if on a phone.
    /// </summary>
    public override bool Sending { get { return Tools.BuildManager.IsStandalone(); } }

    /// <summary>
    /// The phone's IMU, as a Quaternion.
    /// </summary>
    public Quaternion Imu {
      get { return data.vector4s[0]; }
      private set { data.vector4s[0] = value; }
    }

    /// <summary>
    /// Data descriptor is initialized with one Quaternion.
    /// </summary>
    public override void ResetData() {
      data = new Network.Flake(0, 1);
    }

    /// <summary>
    /// If sending, broadcast IMU data.
    /// </summary>
    void UpdateRemote() {
        if (!Sending) return;

        switch (device) {
          case Tools.BuildManager.Device.CARDBOARD:
              Imu = Camera.main.transform.rotation;
              break;
          case Tools.BuildManager.Device.DAYDREAM:
              Imu = UnityEngine.VR.InputTracking.GetLocalRotation(
                UnityEngine.VR.VRNode.CenterEye
              );
              break;
      }
    }
  }
}
