// TrackableComponent.cs
// Created by Holojam Inc. on 12.02.17
// Example Trackable

using UnityEngine;

public class TrackableComponent : Holojam.Tools.Trackable {

  // As an example, expose all the Trackable properties in the inspector.
  // In practice, you probably want to control some or all of these manually in code.

  public string label = "Trackable";
  public string scope = ""; 

  // As an example, allow all the Trackable properties to be publicly settable
  // In practice, you probably want to control some or all of these manually in code.

  public void SetLabel(string label) { this.label = label; }
  public void SetScope(string scope) { this.scope = scope; }

  // Point the property overrides to the public inspector fields

  public override string Label { get { return label; } }
  public override string Scope { get { return scope; } }

  void OnDrawGizmos() {
    DrawGizmoGhost();
  }

  void OnDrawGizmosSelected() {
    Gizmos.color = Color.gray;

    // Pivot
    Holojam.Utility.Drawer.Circle(transform.position, Vector3.up, Vector3.forward, 0.18f);
    Gizmos.DrawLine(transform.position - 0.03f * Vector3.left, transform.position + 0.03f * Vector3.left);
    Gizmos.DrawLine(transform.position - 0.03f * Vector3.forward, transform.position + 0.03f * Vector3.forward);

    // Forward
    Gizmos.DrawRay(transform.position, transform.forward * 0.18f);
  }

  // Draw ghost (in world space) if in local space
  protected void DrawGizmoGhost() {
    if (!LocalSpace || transform.parent == null) return;

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
