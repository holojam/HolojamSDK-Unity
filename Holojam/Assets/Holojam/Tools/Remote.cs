// Remote.cs
// Created by Holojam Inc. on 03.01.17

using UnityEngine;

namespace Holojam.Tools {
  /// <summary>
  /// IMU broadcaster for the Holojam Remote app (only runs on a phone).
  /// </summary>
  public class Remote : Controller{

    public BuildManager.Device device = BuildManager.DEVICE_DEFAULT;

    protected override ProcessDelegate Process { get { return UpdateRemote; } }

    public override string Label { get { return "Remote"; } }
    public override string Scope { get { return Network.Client.SEND_SCOPE; } }
    public override bool Sending { get { return !BuildManager.IsMasterClient(); } }

    public override int QuadCount { get { return 1; } }
    Quaternion Imu { set { SetQuad(0,value); } } // Proxy

    void UpdateRemote() {
        if (!Sending)
          return;

        switch (device) {
          case BuildManager.Device.CARDBOARD:
              Imu = Camera.main.transform.rotation;
              break;
          case BuildManager.Device.DAYDREAM:
              Imu = UnityEngine.VR.InputTracking.GetLocalRotation(
                UnityEngine.VR.VRNode.CenterEye
              );
              break;
      }
    }
  }
}
