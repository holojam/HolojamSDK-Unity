//View.cs
//Created by Aaron C Gaudette on 11.11.16
//Unity representation of flatbuffers object

using UnityEngine;
using System.Collections.Generic;

namespace Holojam.Network {
  /// <summary>
  /// 
  /// </summary>
  public class View : MonoBehaviour {
    public static List<View> instances = new List<View>();
    void OnEnable() { instances.Add(this); }
    void OnDisable() { instances.Remove(this); }

    /// <summary>
    /// 
    /// </summary>
    [HideInInspector] public string scope;

    /// <summary>
    /// 
    /// </summary>
    [HideInInspector] public string source;

    /// <summary>
    /// 
    /// </summary>
    [HideInInspector] public bool sending;

    /// <summary>
    /// 
    /// </summary>
    [HideInInspector] public bool ignoreTracking;

    /// <summary>
    /// 
    /// </summary>
    [HideInInspector] public string label;

    /// <summary>
    /// 
    /// </summary>
    [HideInInspector] public Vector3[] triples;

    /// <summary>
    /// 
    /// </summary>
    [HideInInspector] public Quaternion[] quads;

    /// <summary>
    /// 
    /// </summary>
    [HideInInspector] public float[] floats;

    /// <summary>
    /// 
    /// </summary>
    [HideInInspector] public int[] ints;

    /// <summary>
    /// 
    /// </summary>
    [HideInInspector] public byte[] chars;

    /// <summary>
    /// 
    /// </summary>
    [HideInInspector] public string text;


    bool isTracked = false;
    /// <summary>
    /// 
    /// </summary>
    public bool tracked {
      get {
        return Application.isPlaying && instances.Contains(this)
       && (isTracked || sending);
      }
      set { isTracked = value; }
    }
  }
}
