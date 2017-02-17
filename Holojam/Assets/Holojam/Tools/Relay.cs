// Relay.cs
// Created by Holojam Inc. on 15.02.17

using UnityEngine;

namespace Holojam.Tools {

  /// <summary>
  /// Holojam class for broadcasting tracking/Actor data from a Unity project.
  /// </summary>
  public class Relay : Network.Controller {

    /// <summary>
    /// Automtically selects the canon Actor label depending on the build index.
    /// </summary>
    public override string Label {
      get { return Network.Canon.IndexToLabel(Tools.BuildManager.BUILD_INDEX); }
    }

    public override bool Sending { get { return true; } }

    /// <summary>
    /// Position is sent over the first Vector3.
    /// </summary>
    public Vector3 Position {
      get { return data.triples[0]; }
      set { data.triples[0] = value; }
    }

    /// <summary>
    /// Rotation is sent over the first Quaternion.
    /// </summary>
    public Quaternion Rotation {
      get { return data.quads[0]; }
      set { data.quads[0] = value; }
    }

    protected override ProcessDelegate Process { get { return Load; } }

    public override void ResetData() {
      data = new Network.Flake(1, 1);
    }

    /// <summary>
    /// Override to specify position/rotation data for sending.
    /// </summary>
    protected virtual void Load() { }
  }
}
