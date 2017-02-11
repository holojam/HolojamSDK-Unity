//Trackable.cs
//Created by Aaron C Gaudette on 09.07.16
//Base class for trackable entities

using UnityEngine;

namespace Holojam.Tools {
  /// <summary>
  /// 
  /// </summary>
  public class Trackable : Controller {

    /// <summary>
    /// 
    /// </summary>
    [SerializeField]
    private string label = "Trackable";

    /// <summary>
    /// 
    /// </summary>
    [SerializeField]
    public string scope = "";

    /// <summary>
    /// 
    /// </summary>
    [SerializeField]
    public bool localSpace = false;

    /// <summary>
    /// 
    /// </summary>
    public override int tripleCount { get { return 1; } }

    /// <summary>
    /// 
    /// </summary>
    public override int quadCount { get { return 1; } }

    //Proxies
    /// <summary>
    /// 
    /// </summary>
    public Vector3 rawPosition {
      get { return GetTriple(0); }
      protected set { SetTriple(0, value); }
    }

    /// <summary>
    /// 
    /// </summary>
    public Quaternion rawRotation {
      get { return GetQuad(0); }
      protected set { SetQuad(0, value); }
    }

    //Accessors in case modification needs to be made to the raw data (like smoothing)
    /// <summary>
    /// 
    /// </summary>
    public Vector3 trackedPosition {
      get {
        return localSpace && transform.parent != null ?
           transform.parent.TransformPoint(rawPosition) : rawPosition;
      }
    }

    /// <summary>
    /// 
    /// </summary>
    public Quaternion trackedRotation {
      get {
        return localSpace && transform.parent != null ?
           transform.parent.rotation * rawRotation : rawRotation;
      }
    }

    protected override ProcessDelegate Process { get { return UpdateTracking; } }

    public override string labelField { get { return label; } }
    public override string scopeField { get { return scope; } }

    /// <summary>
    /// 
    /// </summary>
    public override bool isSending { get { return false; } }

    //Override in derived classes
    protected virtual void UpdateTracking() {
      //By default, assigns position and rotation injectively
      if (view.tracked) {
        transform.position = trackedPosition;
        transform.rotation = trackedRotation;
      }
      //Untracked maintains last known position and rotation
    }

    void OnDrawGizmos() {
      DrawGizmoGhost();
    }
    void OnDrawGizmosSelected() {
      Gizmos.color = Color.gray;
      //Pivot
      Utility.Drawer.Circle(transform.position, Vector3.up, Vector3.forward, 0.18f);
      Gizmos.DrawLine(transform.position - 0.03f * Vector3.left, transform.position + 0.03f * Vector3.left);
      Gizmos.DrawLine(transform.position - 0.03f * Vector3.forward, transform.position + 0.03f * Vector3.forward);
      //Forward
      Gizmos.DrawRay(transform.position, transform.forward * 0.18f);
    }
    //Draw ghost (in world space) if in local space
    protected void DrawGizmoGhost() {
      if (!localSpace || transform.parent == null) return;
      Gizmos.color = Color.gray;
      Gizmos.DrawLine(
         rawPosition - 0.03f * Vector3.left,
         rawPosition + 0.03f * Vector3.left
      );
      Gizmos.DrawLine(
         rawPosition - 0.03f * Vector3.forward,
         rawPosition + 0.03f * Vector3.forward
      );
      Gizmos.DrawLine(rawPosition - 0.03f * Vector3.up, rawPosition + 0.03f * Vector3.up);
    }
  }
}
