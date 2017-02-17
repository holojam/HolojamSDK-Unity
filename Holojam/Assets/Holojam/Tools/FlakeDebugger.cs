// FlakeDebugger.cs
// Created by Holojam Inc. on 13.07.16

using UnityEngine;

namespace Holojam.Tools {

  /// <summary>
  /// Read-only data dump of a View for debugging.
  /// </summary>
  public sealed class FlakeDebugger : Network.Controller {

    public string scope, label;

    [Space(8)]
    public string origin;
    public bool isTracked;

    public Vector3[] triples;
    public Quaternion[] quads;
    public float[] floats;
    public int[] ints;
    public byte[] chars;
    public string text;

    public override string Scope { get { return scope; } }
    public override string Label { get { return label; } }
    public override bool Sending { get { return false; } }

    protected override ProcessDelegate Process { get { return Refresh; } }

    void Refresh() {
      origin = Source;
      isTracked = Tracked;

      triples = data.triples;
      quads = data.quads;
      floats = data.floats;
      ints = data.ints;
      chars = data.chars;
      text = data.text;
    }
  }
}
