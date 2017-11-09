// SharedData.cs
// Created by Holojam Inc. on 08.11.17

using UnityEngine;

namespace Holojam.Components {

  public class SharedData : Network.Controller {

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

    public void SetScope(string scope) {
      this.scope = scope;
    }

    public override string Scope {
      get { return ""; }
    }

    public override bool Sending {
      get { return Tools.BuildManager.IsMasterClient(); }
    }
  }
}
