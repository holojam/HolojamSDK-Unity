// Tracker.cs
// Created by Holojam Inc. on 11.11.16

using UnityEngine;

namespace Holojam.Tools{

  /// <summary>
  /// Unity representation of a tracking camera, used for placing a Martian Actor in the space.
  /// Receives (extra) data from Holoscope.
  /// </summary>
  public sealed class Tracker : Controller{

    /// <summary>
    /// Position of the camera in the scene.
    /// </summary>
    public Vector2 origin;

    /// <summary>
    /// Height of the camera in meters.
    /// </summary>
    public float height = 1;

    /// <summary>
    /// Up/down angle of the camera in degrees.
    /// </summary>
    [Range(-90, 90)] public float angle = 0; // Degrees

    protected override ProcessDelegate Process { get { return UpdateFrustum; } }

    /// <summary>
    /// Proxy for the first float (Martian stem).
    /// </summary>
    public float Stem { get { return GetFloat(0); } }

    public override string Label {get { return "ExtraData"; } }
    public override string Scope { get { return "Holoscope"; } }
    public override bool Sending { get { return false; } }

    public override int FloatCount { get { return 1; } }

    void UpdateFrustum() {
        transform.position = new Vector3(origin.x, height, origin.y);
        transform.rotation = Quaternion.AngleAxis(-angle, Vector3.right);
    }

    /// <summary>
    /// Localize an input position within this reference frame.
    /// </summary>
    public Vector3 Localize(Vector3 position) {
        return transform.TransformPoint(position);
    }

    /// <summary>
    /// Localize an input rotation within this reference frame.
    /// </summary>
    public Quaternion Localize(Quaternion rotation) {
        return Quaternion.Inverse(transform.rotation) * rotation;
    }
  }
}
