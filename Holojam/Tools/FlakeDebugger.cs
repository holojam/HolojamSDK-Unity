// FlakeDebugger.cs
// Created by Holojam Inc. on 13.07.16

using UnityEngine;

namespace Holojam.Tools {

  /// <summary>
  /// Read-only data dump of a Controller for debugging.
  /// </summary>
  public sealed class FlakeDebugger : Network.Controller {

    public string scope, label;

    [Space(8)]
    public float deltaTime;
    public bool updatedThisFrame;

    [Space(8)]
    public string origin;
    public bool tracked;

    public Vector3[] vector3s;
    public Quaternion[] vector4s;
    public float[] floats;
    public int[] ints;
    public byte[] bytes;
    public string text;

    public sealed override string Scope { get { return scope; } }
    public sealed override string Label { get { return label; } }
    public sealed override bool Sending { get { return false; } }

    protected sealed override ProcessDelegate Process { get { return Refresh; } }

    void Refresh() {
      deltaTime = DeltaTime();
      updatedThisFrame = UpdatedThisFrame;

      origin = Source;
      tracked = Tracked;

      vector3s = data.vector3s;
      vector4s = data.vector4s;
      floats = data.floats;
      ints = data.ints;
      bytes = data.bytes;
      text = data.text;
    }
  }
}
