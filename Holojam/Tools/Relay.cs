// Relay.cs
// Created by Holojam Inc. on 15.02.17

using UnityEngine;

namespace Holojam.Tools {

  /// <summary>
  /// Abstract class for broadcasting tracking/Actor data from a Unity project.
  /// </summary>
  public abstract class Relay : Network.Controller {

    /// <summary>
    /// String to append to the label. Override.
    /// </summary>
    public virtual string Extra { get; }

    /// <summary>
    /// Automatically selects the canon Actor label depending on the build index.
    /// Override Extra to append.
    /// </summary>
    public sealed override string Label {
      get {
        return Network.Canon.IndexToLabel(Tools.BuildManager.BUILD_INDEX, Extra);
      }
    }

    /// <summary>
    /// Relays should never accept incoming data.
    /// </summary>
    public sealed override bool Deaf { get { return true; } }

    /// <summary>
    /// Position is sent over the first Vector3.
    /// </summary>
    public Vector3 Position {
      get { return data.vector3s[0]; }
      set { data.vector3s[0] = value; }
    }

    /// <summary>
    /// Rotation is sent over the first Quaternion.
    /// </summary>
    public Quaternion Rotation {
      get { return data.vector4s[0]; }
      set { data.vector4s[0] = value; }
    }

    protected sealed override ProcessDelegate Process { get { return Load; } }

    /// <summary>
    /// Data descriptor is initialized with one Vector3 and one Quaternion.
    /// </summary>
    public override void ResetData() {
      data = new Network.Flake(1, 1);
    }

    /// <summary>
    /// Process method, called on Update().
    /// Override to specify position/rotation data for sending.
    /// </summary>
    protected abstract void Load();
  }
}
