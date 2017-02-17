// SynchronizedTransform.cs
// Created by Holojam Inc. on 26.01.17
// Example Synchronizable

using UnityEngine;

public class SynchronizedTransform : Holojam.Tools.Synchronizable {

  [SerializeField] string label = "Synchronizable";
  [SerializeField] string scope = "";

  public override string Label { get { return label; } }
  public override string Scope { get { return scope; } }

  // Proxies
  public Vector3 Position {
    get { return data.triples[0]; }
    set { data.triples[0] = value; }
  }
  public Quaternion Rotation {
    get { return data.quads[0]; }
    set { data.quads[0] = value; }
  }
  public Vector3 Scale{
    get { return data.triples[1]; }
    set { data.triples[1] = value; }
  }

  public override void ResetData() {
    data = new Holojam.Network.Flake(2, 1);
  }

  protected override void Sync() {
    if (Sending) {
      Position = transform.position;
      Rotation = transform.rotation;
      Scale = transform.localScale;
    } else {
      transform.position = Position;
      transform.rotation = Rotation;
      transform.localScale = Scale;
    }
  }
}
