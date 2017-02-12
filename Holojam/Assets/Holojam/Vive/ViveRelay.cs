// ViveRelay.cs
// Created by Holojam Inc. on 26.01.17

using UnityEngine;

namespace Holojam.Vive {
  /// <summary>
  /// Add this component to the SteamVR prefab on Camera (eye)
  /// to synchronize HTC Vives within the Actor system.
  /// </summary>
  public sealed class ViveRelay : Tools.Controller {
    protected override ProcessDelegate Process { get { return Relay; } }

    public override string Label {
      get { return Network.Canon.IndexToLabel(Tools.BuildManager.BUILD_INDEX); }
    }
    public override string Scope { get { return "HolojamVive"; } }
    public override bool Sending { get { return !Holojam.Tools.BuildManager.IsMasterClient(); } }

    public override int TripleCount { get { return 1; } }
    public override int QuadCount { get { return 1; } }

    void Relay() {
      SetTriple(0, transform.position);
      SetQuad(0, transform.rotation);
    }
  }
}
