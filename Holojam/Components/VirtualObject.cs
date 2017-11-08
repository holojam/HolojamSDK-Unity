// VirtualObject.cs
// Created by Holojam Inc. on 08.11.17

using UnityEngine;

namespace Holojam.Components {

  public class VirtualObject : Tools.Trackable {

    [SerializeField] string label = "MyVirtualObject";
    [SerializeField] int owner = 1;

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
      get { return owner == Tools.BuildManager.BUILD_INDEX; }
    }
  }
}
