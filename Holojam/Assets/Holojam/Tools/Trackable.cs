// Trackable.cs
// Created by Holojam Inc. on 09.07.16

using UnityEngine;

namespace Holojam.Tools {

  /// <summary>
  /// Base class for trackable (read-only position and rotation) entities.
  /// </summary>
  public abstract class Trackable : Network.Controller {

    readonly Utility.Smoothing POS_SMOOTHING = new Utility.Smoothing(.1f,1.4f);
    readonly Utility.Smoothing ROT_SMOOTHING = new Utility.Smoothing(1.6f,1.1f);

    /// <summary>
    /// Determines whether this object is affected by hierarchy.
    /// Beta field. Should not be public.
    /// </summary>
    public bool localSpace = false;

    /// <summary>
    /// Apply a subtle smoothing function to level out any minor rough edges.
    /// </summary>
    [SerializeField] bool smooth = false;

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
          Utility.Smoothing.Smooth(position, ref lastPosition, POS_SMOOTHING) :
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
          Utility.Smoothing.Smooth(rotation, ref lastRotation, ROT_SMOOTHING) :
          rotation;
      }
    }
    Quaternion lastRotation;

    protected override ProcessDelegate Process { get { return UpdateTracking; } }

    /// <summary>
    /// Trackables are read-only.
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

    void OnDrawGizmos() {
      DrawGizmoGhost();
    }
    void OnDrawGizmosSelected() {
      Gizmos.color = Color.gray;
      // Pivot
      Utility.Drawer.Circle(transform.position, Vector3.up, Vector3.forward, 0.18f);
      Gizmos.DrawLine(transform.position - 0.03f * Vector3.left, transform.position + 0.03f * Vector3.left);
      Gizmos.DrawLine(transform.position - 0.03f * Vector3.forward, transform.position + 0.03f * Vector3.forward);
      // Forward
      Gizmos.DrawRay(transform.position, transform.forward * 0.18f);
    }
    // Draw ghost (in world space) if in local space
    protected void DrawGizmoGhost() {
      if (!localSpace || transform.parent == null) return;
      Gizmos.color = Color.gray;
      Gizmos.DrawLine(
         RawPosition - 0.03f * Vector3.left,
         RawPosition + 0.03f * Vector3.left
      );
      Gizmos.DrawLine(
         RawPosition - 0.03f * Vector3.forward,
         RawPosition + 0.03f * Vector3.forward
      );
      Gizmos.DrawLine(RawPosition - 0.03f * Vector3.up, RawPosition + 0.03f * Vector3.up);
    }
  }
}
