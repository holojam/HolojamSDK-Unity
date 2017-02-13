// Trackable.cs
// Created by Holojam Inc. on 09.07.16

using UnityEngine;

namespace Holojam.Tools {

  /// <summary>
  /// Base class for trackable (read-only position and rotation) entities.
  /// </summary>
  public abstract class Trackable : Controller {

    /// <summary>
    /// Determines whether this object is affected by hierarchy.
    /// Beta field. Should not be public.
    /// </summary>
    public bool localSpace = false;

    public override int TripleCount { get { return 1; } }
    public override int QuadCount { get { return 1; } }

    /// <summary>
    /// Proxy for the first triple (raw position).
    /// </summary>
    public Vector3 RawPosition {
      get { return GetTriple(0); }
      protected set { SetTriple(0, value); }
    }

    /// <summary>
    /// Proxy for the first quad (raw rotation).
    /// </summary>
    public Quaternion RawRotation {
      get { return GetQuad(0); }
      protected set { SetQuad(0, value); }
    }

    //Accessors in case modification needs to be made to the raw data (like smoothing)
    /// <summary>
    /// Second layer accessor in case modification needs to be made to the raw data
    /// (e.g. smoothing). In general, use this property.
    /// </summary>
    public Vector3 TrackedPosition {
      get {
        return localSpace && transform.parent != null ?
           transform.parent.TransformPoint(RawPosition) : RawPosition;
      }
    }

    /// <summary>
    /// Second layer accessor in case modification needs to be made to the raw data
    /// (e.g. smoothing). In general, use this property.
    /// </summary>
    public Quaternion TrackedRotation {
      get {
        return localSpace && transform.parent != null ?
           transform.parent.rotation * RawRotation : RawRotation;
      }
    }

    protected override ProcessDelegate Process { get { return UpdateTracking; } }

    /// <summary>
    /// Trackables are read-only.
    /// </summary>
    public override bool Sending { get { return false; } }

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
