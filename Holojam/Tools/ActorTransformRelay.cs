// ActorTransformRelay.cs
// Created by Holojam Inc. on 26.01.17

using UnityEngine;

namespace Holojam.Tools {

  /// <summary>
  /// Sends Actor position/orientation data from a transform.
  /// </summary>
  public class ActorTransformRelay : Tools.Relay {

    /// <summary>
    /// Send over the build label.
    /// </summary>
    public sealed override string Extra { get { return null; } }

    /// <summary>
    /// Don't send on master clients or spectators.
    /// </summary>
    public sealed override bool Sending {
      get {
        return !Holojam.Tools.BuildManager.IsMasterClient()
          && !Holojam.Tools.BuildManager.IsSpectator();
      }
    }

    /// <summary>
    /// Send position and rotation every update
    /// </summary>
    protected override void Load() {
      Position = transform.position;
      Rotation = transform.rotation;
    }
  }
}
