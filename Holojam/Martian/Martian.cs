// Martian.cs
// Created by Holojam Inc. on 15.02.17

using UnityEngine;

namespace Holojam.Martian {

  /// <summary>
  /// Internal descriptor for input (raw) Martian data.
  /// </summary>
  internal sealed class Martian : Network.Controller {

    /// <summary>"Holoscope"</summary>
    public override string Scope { get { return "Holoscope"; } }

    /// <summary>
    /// Listen on the raw Martian label associated with the build index.
    /// </summary>
    public override string Label {
      get { return Network.Canon.IndexToLabel(Tools.BuildManager.BUILD_INDEX, "Raw"); }
    }

    /// <summary>
    /// Input-only.
    /// </summary>
    public override bool Sending { get { return false; } }

    /// <summary>
    /// Data descriptor is initialized with two Vector3s.
    /// </summary>
    public override void ResetData() {
      data = new Network.Flake(2);
    }

    /// <summary>
    /// Holoscope raw input Vector3.
    /// </summary>
    public Vector3 Left { get { return data.vector3s[0]; } }

    /// <summary>
    /// Holoscope raw input Vector3.
    /// </summary>
    public Vector3 Right { get { return data.vector3s[1]; } }
  }
}
