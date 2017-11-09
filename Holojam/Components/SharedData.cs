// SharedData.cs
// Created by Holojam Inc. on 08.11.17

using UnityEngine;

namespace Holojam.Components {

  public class SharedData : Network.Controller {

    string label;

    public override string Label {
      get { return label; }
    }

    public void SetLabel(string label) {
      this.label = label;
    }

    string scope = "";

    public override string Scope {
      get { return scope; }
    }

    public void SetScope(string scope) {
      this.scope = scope;
    }

    /// <summary>
    /// Host the data if this is the master client.
    /// </summary>
    public override bool Sending {
      get { return Tools.BuildManager.IsMasterClient(); }
    }

    protected override void Awake() {
      base.Awake();
      label = Labeler.GenerateLabel(this.gameObject);
    }
  }
}
