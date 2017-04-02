// SynchronizableComponent.cs
// Created by Holojam Inc. on 26.01.17
// Example Synchronizable

using UnityEngine;

public class SynchronizableComponent : Holojam.Tools.SynchronizableTrackable {

  // As an example, expose all the Synchronizable properties in the inspector.
  // In practice you probably don't want to do this.

  public string label = "Synchronizable";
  public string scope = "";

  public bool host = true;
  public bool autoHost = false;

  // Point the property overrides to the public inspector fields

  public override string Label { get { return label; } }
  public override string Scope { get { return scope; } }

  public override bool Host { get { return host; } }
  public override bool AutoHost { get { return autoHost; } }

  // Proxy (proper data hiding)
  public Vector3 Scale {
    get { return data.vector3s[1]; }
    private set { data.vector3s[1] = value; }
  }

  // Add the scale vector to Trackable, which by default only contains position/rotation
  public override void ResetData() {
    data = new Holojam.Network.Flake(2, 1);
  }

  // Override Sync() to include the scale vector
  protected override void Sync() {
    base.Sync();

    if (Sending) {
      Scale = transform.localScale;
    } else {
      transform.localScale = Scale;
    }
  }

  protected override void Update() {
    if (autoHost) host = Sending; // Lock host flag
    base.Update();
  }
}
