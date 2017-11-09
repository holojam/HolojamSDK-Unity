// VirtualObject.cs
// Created by Holojam Inc. on 08.11.17

using UnityEngine;

namespace Holojam.Components {

  public class VirtualObject : Tools.Trackable {

    [SerializeField] Tools.Actor owner = null;

    string label;

    protected override void Awake() {
      base.Awake();
      label = Labeler.Instance.GenerateLabel(this.gameObject);
    }

    public void SetLabel(string label) {
      this.label = label;
    }

    public override string Label {
      get { return label; }
    }

    public override string Scope {
      get { return ""; }
    }

    /// <summary>
    /// Override Trackable's read-only behavior
    /// </summary>
    public override bool Sending {
      get { return owner == Tools.BuildManager.BUILD_ACTOR; }
    }

    protected override void UpdateTracking() {
      if (Sending) {
        RawPosition = transform.position;
        RawRotation = transform.rotation;
      } else if (Tracked) {
        transform.position = TrackedPosition;
        transform.rotation = TrackedRotation;
      }
    }
  }
}
