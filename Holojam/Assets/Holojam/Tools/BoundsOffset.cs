// BoundsOffset.cs
// Created by Holojam Inc. on 23.06.16

using UnityEngine;

namespace Holojam.Tools{
  /// <summary>
  /// Automatically centers object to holobounds and offsets.
  /// Deprecated.
  /// </summary>
  [ExecuteInEditMode]
  public class BoundsOffset : MonoBehaviour {
    public Holobounds holobounds;
    public Vector3 offset = Vector3.zero;

    void LateUpdate() {
      if (holobounds != null) {
        transform.position = holobounds.Offset(offset);
        transform.rotation = holobounds.rotation;
      }
    }
  }
}
