// Martian.cs
// Created by Holojam Inc. on 15.02.17

using UnityEngine;

namespace Holojam.Martian {

  /// <summary>
  /// Internal descriptor for input (raw) Martian data.
  /// </summary>
  internal sealed class Martian : Network.Controller {

    public override string Scope { get { return "Holoscope"; } }
    public override string Label {
      get { return Network.Canon.IndexToLabel(Tools.BuildManager.BUILD_INDEX, true); }
    }
    public override bool Sending { get { return false; } }

    public override void ResetData() {
      data = new Network.Flake(2);
    }

    /// <summary>
    /// Holoscope raw input Vector3.
    /// </summary>
    public Vector3 Left { get { return data.triples[0]; } }

    /// <summary>
    /// Holoscope raw input Vector3.
    /// </summary>
    public Vector3 Right { get { return data.triples[1]; } }

    protected override ProcessDelegate Process {
      get { return () => { }; }
    }
  }
}
