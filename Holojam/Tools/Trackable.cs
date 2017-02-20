// Trackable.cs
// Created by Holojam Inc. on 09.07.16

using UnityEngine;
using Holojam.Utility;

namespace Holojam.Tools {

  /// <summary>
  /// Base class for trackable (read-only position and rotation) entities.
  /// </summary>
  public abstract class Trackable : Network.Controller {

    /// <summary>
    /// Determines whether this object is affected by hierarchy.
    /// Beta/deprecated field.
    /// </summary>
    [SerializeField] bool localSpace = false;

    /// <summary>
    /// Is this object is affected by hierarchy?
    /// Beta/deprecated property.
    /// </summary>
    public bool LocalSpace { get { return localSpace; } }

    /// <summary>
    /// Apply a subtle smoothing function to level out any rough edges.
    /// </summary>
    [SerializeField] protected bool smooth = false;

    /// <summary>
    /// The raw position data (from the network), before localization/smoothing.
    /// </summary>
    public Vector3 RawPosition {
      get { return data.vector3s[0]; }
      protected set { data.vector3s[0] = value; }
    }

    /// <summary>
    /// The raw rotation data (from the network), before localization/smoothing.
    /// </summary>
    public Quaternion RawRotation {
      get { return data.vector4s[0]; }
      protected set { data.vector4s[0] = value; }
    }

    /// <summary>
    /// Second layer accessor in case modification needs to be made to the raw data.
    /// Here, both smoothing and local space offset is being applied. In general, use
    /// this property over RawPosition.
    /// </summary>
    public Vector3 TrackedPosition {
      get {
        Vector3 position = localSpace && transform.parent != null ?
          transform.parent.TransformPoint(RawPosition) : RawPosition;

        return smooth ?
          smoother.Smooth(position, ref lastPosition, DeltaTime()) :
          position;
      }
    }
    Vector3 lastPosition;

    /// <summary>
    /// Second layer accessor in case modification needs to be made to the raw data.
    /// Here, both smoothing and local space offset is being applied. In general, use
    /// use this property over RawRotation.
    /// </summary>
    public Quaternion TrackedRotation {
      get {
        Quaternion rotation = localSpace && transform.parent != null ?
           transform.parent.rotation * RawRotation : RawRotation;

        return smooth ?
          smoother.Smooth(rotation, ref lastRotation, DeltaTime()) :
          rotation;
      }
    }
    Quaternion lastRotation;

    protected sealed override ProcessDelegate Process { get { return UpdateTracking; } }

    AdaptiveSmoother smoother = new AdaptiveSmoother();

    /// <summary>
    /// Trackables are read-only by default.
    /// </summary>
    public override bool Sending { get { return false; } }

    /// <summary>
    /// Data descriptor is initialized with one Vector3 and one Quaternion.
    /// </summary>
    public override void ResetData() {
      data = new Network.Flake(1, 1);
    }

    /// <summary>
    /// Override for more complex behavior.
    /// By default, assigns position and rotation injectively.
    /// Untracked maintains last known position and rotation.
    /// </summary>
    protected virtual void UpdateTracking() {
      if (Tracked) {
        transform.position = TrackedPosition;
        transform.rotation = TrackedRotation;
      }
    }
  }
}
