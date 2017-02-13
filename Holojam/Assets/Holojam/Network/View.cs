// View.cs
// Created by Holojam Inc. on 11.11.16

using UnityEngine;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Holojam.Network {

  /// <summary>
  /// Unity component representation of a Holojam flake, with a custom editor inspector.
  /// </summary>
  public sealed class View : MonoBehaviour {

    /// <summary>
    /// Static read-only list of all Views in the scene.
    /// </summary>
    internal static ReadOnlyCollection<View> All {
      get { return instances.AsReadOnly(); }
    }
    static List<View> instances = new List<View>();

    void OnEnable() { instances.Add(this); }
    void OnDisable() { instances.Remove(this); }

    /// <summary>
    /// Read-only string containing Holojam network origin (user@host).
    /// </summary>
    public string Source {
      get { return source; }
      internal set { source = value; }
    }
    string source;

    /// <summary>
    /// Read-only bool indicating if the View received fresh data this frame.
    /// </summary>
    public bool Tracked {
      get {
        return Application.isPlaying && instances.Contains(this)
       && (tracked || sending);
      }
      internal set { tracked = value; }
    }
    bool tracked = false;

    /// <summary>
    /// Holojam network 'namespace' string.
    /// </summary>
    [HideInInspector] public string scope;

    /// <summary>
    /// This View's unique identifier (string).
    /// </summary>
    [HideInInspector] public string label;

    /// <summary>
    /// A read/write flag indicating whether or not this View is
    /// broadcasting or receiving.
    /// </summary>
    [HideInInspector] public bool sending;

    /// <summary>
    /// A read/write flag indicating whether or not this View
    /// should respond to updates.
    /// </summary>
    [HideInInspector] public bool deaf;

    /// <summary>
    /// An optional array of Vector3s for storing/staging data.
    /// </summary>
    [HideInInspector] public Vector3[] triples;

    /// <summary>
    /// An optional array of Quaternions for storing/staging data.
    /// </summary>
    [HideInInspector] public Quaternion[] quads;

    /// <summary>
    /// An optional array of floats for storing/staging data.
    /// </summary>
    [HideInInspector] public float[] floats;

    /// <summary>
    /// An optional array of ints for storing/staging data.
    /// </summary>
    [HideInInspector] public int[] ints;

    /// <summary>
    /// An optional array of bytes for storing/staging data.
    /// </summary>
    [HideInInspector] public byte[] chars;

    /// <summary>
    /// An optional string for storing/staging data.
    /// </summary>
    [HideInInspector] public string text;
  }
}
