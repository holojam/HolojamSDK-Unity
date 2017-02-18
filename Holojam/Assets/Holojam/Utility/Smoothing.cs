// Smoothing.cs
// Created by Holojam Inc. on 17.02.17

using UnityEngine;

namespace Holojam.Utility {

  /// <summary>
  /// Class container for smoothing variables.
  /// </summary>
  [System.Serializable]
  public class Smoothing {

    /// <summary>
    /// Scaling factor for the input signal. Lower values emphasize "snapping".
    /// In the case of position, this is meters. In the case of rotation, the signal
    /// ranges from 0 to 1 (a full 180 degree rotation).
    /// </summary>
    public float cap;

    /// <summary>
    /// Exponent to apply after scaling. A value of 1 produces linear smoothing.
    /// Higher values flatten the curve, smoothing more when the difference is small.
    /// </summary>
    public float pow;

    public Smoothing(float cap, float pow) {
      this.cap = cap;
      this.pow = pow;
    }

    /// <summary>
    /// Smooth a Vector3 given a reference to its last state. Generally this is called
    /// within an update function.
    /// </summary>
    /// <param name="target"></param>
    /// <param name="last"></param>
    /// <param name="smoothing"></param>
    /// <returns>A smoothed version of the input.</returns>
    public static Vector3 Smooth(Vector3 target, ref Vector3 last, Smoothing smoothing) {
      target = Vector3.Lerp(last, target, Mathf.Pow(
        Mathf.Min(1, (last - target).magnitude / smoothing.cap), smoothing.pow
      ));
      last = target;
      return target;
    }

    /// <summary>
    /// Smooth a Quaternion given a reference to its last state. Generally this is called
    /// within an update function.
    /// </summary>
    /// <param name="target"></param>
    /// <param name="last"></param>
    /// <param name="smoothing"></param>
    /// <returns>A smoothed version of the input.</returns>
    public static Quaternion Smooth(Quaternion target, ref Quaternion last, Smoothing smoothing) {
      float difference = .5f * Quaternion.Dot(last, target) + .5f;
      target = Quaternion.Slerp(last, target, Mathf.Pow(
        Mathf.Min(1, difference / smoothing.cap), smoothing.pow
      ));
      last = target;
      return target;
    }
  }
}
