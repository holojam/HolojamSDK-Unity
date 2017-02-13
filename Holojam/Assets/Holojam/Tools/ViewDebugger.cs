// ViewDebugger.cs
// Created by Holojam Inc. on 13.07.16

using UnityEngine;

namespace Holojam.Tools {

  /// <summary>
  /// Read-only data dump of a View for debugging.
  /// </summary>
  public sealed class ViewDebugger : MonoBehaviour {

    public string label, scope;
    public bool deaf;

    [Space(8)]
    public string source;
    public bool tracked;

    public Vector3[] triples;
    public Quaternion[] quads;
    public float[] floats;
    public int[] ints;
    public byte[] chars;
    public string text;

    Network.View view;
    void Awake() {
      view = gameObject.AddComponent<Network.View>() as Network.View;
    }

    void Update() {
      view.label = label;
      view.scope = scope;

      source = view.Source;
      tracked = view.Tracked;
      deaf = view.deaf;

      triples = view.triples;
      quads = view.quads;
      floats = view.floats;
      ints = view.ints;
      chars = view.chars;
      text = view.text;
    }
  }
}
