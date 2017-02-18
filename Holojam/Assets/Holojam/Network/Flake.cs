// Flake.cs
// Created by Holojam Inc. on 12.02.17

using UnityEngine;

namespace Holojam.Network {

  /// <summary>
  /// Pure (unlabeled) data container as per the Holojam protocol.
  /// </summary>
  public sealed class Flake {

    /// <summary>
    /// An optional array of Vector3s for storing/staging data.
    /// </summary>
    public Vector3[] vector3s;

    /// <summary>
    /// An optional array of Quaternions for storing/staging data.
    /// </summary>
    public Quaternion[] vector4s;

    /// <summary>
    /// An optional array of floats for storing/staging data.
    /// </summary>
    public float[] floats;

    /// <summary>
    /// An optional array of ints for storing/staging data.
    /// </summary>
    public int[] ints;

    /// <summary>
    /// An optional array of bytes for storing/staging data.
    /// </summary>
    public byte[] bytes;

    /// <summary>
    /// An optional string for storing/staging data.
    /// </summary>
    public string text;

    /// <summary>
    /// The parameters passed to this constructor determine whether the
    /// optional members will be initialized as null (default) or to a
    /// specific size. This is for convenience--tampering with the members
    /// outside of the constructor is allowed.
    /// </summary>
    /// <param name="vector3Count"></param>
    /// <param name="vector4Count"></param>
    /// <param name="floatCount"></param>
    /// <param name="intCount"></param>
    /// <param name="byteCount"></param>
    /// <param name="hasText"></param>
    public Flake(
      int vector3Count = 0, int vector4Count = 0,
      int floatCount = 0, int intCount = 0, int byteCount = 0,
      bool hasText = false
    ) {
      // Allocate
      vector3s = vector3Count > 0 ? new Vector3[vector3Count] : null;
      vector4s = vector4Count > 0 ? new Quaternion[vector4Count] : null;
      floats = floatCount > 0 ? new float[floatCount] : null;
      ints = intCount > 0 ? new int[intCount] : null;
      bytes = byteCount > 0 ? new byte[byteCount] : null;
      text = hasText ? "" : null;
    }
  }
}
