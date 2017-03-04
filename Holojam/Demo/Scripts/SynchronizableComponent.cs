// SynchronizableComponent.cs
// Created by Holojam Inc. on 26.01.17
// Example Synchronizable

using UnityEngine;

public class SynchronizableComponent : Holojam.Tools.SynchronizableTrackable {

  [SerializeField] string label = "Synchronizable";
  [SerializeField] string scope = "";

  [SerializeField] bool host = true;
  [SerializeField] bool autoHost = false;

  public override string Label { get { return label; } }
  public override string Scope { get { return scope; } }

  public override bool Host { get { return host; } }
  public override bool AutoHost { get { return autoHost; } }

  // Proxy
  public Vector3 Scale{
    get { return data.vector3s[1]; }
    private set { data.vector3s[1] = value; }
  }

  // Add the scale vector to Trackable
  public override void ResetData() {
    data = new Holojam.Network.Flake(2, 1);
  }

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
