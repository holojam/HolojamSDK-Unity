// ViveRelay.cs
// Created by Holojam Inc. on 26.01.17

using UnityEngine;

namespace Holojam.Vive {

  /// <summary>
  /// Add this component to the CameraRig prefab on Camera (eye)
  /// to synchronize HTC Vives within the Actor system.
  /// </summary>
  public sealed class ViveRelay : Tools.Relay {

    /// <summary>
    /// The ViveRelay doesn't send on master clients.
    /// </summary>
    public override bool Sending { get { return !Holojam.Tools.BuildManager.IsMasterClient(); } }

    /// <summary>
    /// Send position and rotation every update.
    /// </summary>
    protected override void Load() {
      Position = transform.position;
      Rotation = transform.rotation;
    }
  }
}
