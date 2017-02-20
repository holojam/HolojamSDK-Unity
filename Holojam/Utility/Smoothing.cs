// Smoothing.cs
// Created by Holojam Inc. on 17.02.17

using UnityEngine;

namespace Holojam.Utility {

  /// <summary>
  /// Position/rotation smoothing class, optimized for low-latency and jitter (noise) reduction.
  /// Useful for environments where accuracy is more important than interpolation.
  /// </summary>
  [System.Serializable]
  public class AccurateSmoother {

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

    public AccurateSmoother(float cap, float pow) {
      this.cap = cap;
      this.pow = pow;
    }

    /// <summary>
    /// Smooth a Vector3 given a reference to its last state. Generally this is called
    /// within an update function.
    /// </summary>
    /// <param name="target"></param>
    /// <param name="last"></param>
    /// <returns>A smoothed version of the input.</returns>
    public Vector3 Smooth(Vector3 target, ref Vector3 last) {
      target = Vector3.Lerp(last, target, Mathf.Pow(
        Mathf.Min(1, (last - target).magnitude / cap), pow
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
    /// <returns>A smoothed version of the input.</returns>
    public Quaternion Smooth(Quaternion target, ref Quaternion last) {
      float difference = .5f * Quaternion.Dot(last, target) + .5f;
      target = Quaternion.Slerp(last, target, Mathf.Pow(
        Mathf.Min(1, difference / cap), pow
      ));
      last = target;
      return target;
    }
  }

  /// <summary>
  /// Position/rotation smoothing class, optimized for steady animation in
  /// variable-framerate environments.
  /// Adaptively applies more or less smoothing based on update rate.
  /// This has been pre-tuned for general use.
  /// </summary>
  public class AdaptiveSmoother {

    /// <summary>
    /// Range of FPS values covered by the smoothing function.
    /// </summary>
    readonly Vector2 FPS_RANGE = new Vector2(10, 60);

    /// <summary>
    /// Range of scales to interpolate between. Larger values are more true to the input signal.
    /// </summary>
    readonly Vector2 POSITION_SCALE = new Vector2(10, 30);

    /// <summary>
    /// Range of scales to interpolate between. Larger values are more true to the input signal.
    /// </summary>
    readonly Vector2 ROTATION_SCALE = new Vector2(10, 30);

    /// <summary>
    /// Scaling factor for the interpolator. Larger values are jerkier but catch
    /// frame drops or hangs more readily.
    /// </summary>
    const int FACTOR_SCALE = 8;

    /// <summary>
    /// Don't smooth if the update took longer than this many ms.
    /// </summary>
    const int TIMEOUT = 180; //ms

    /// <summary>
    /// Don't smooth if the position difference ran above this many meters.
    /// </summary>
    const float DISTANCE_CAP = 1; //m

    /// <summary>
    /// Don't smooth if the rotation dot ran below this value.
    /// </summary>
    const float DOT_MIN = -.5f;

    float positionFactor, rotationFactor;

    /// <summary>
    /// Smooth a Vector3 given a reference to its last state and a delta time.
    /// Generally this is called within an update function.
    /// </summary>
    /// <param name="target"></param>
    /// <param name="last"></param>
    /// <param name="delta"></param>
    /// <returns>A smoothed version of the input.</returns>
    public Vector3 Smooth(Vector3 target, ref Vector3 last, float delta) {
      if (delta > TIMEOUT / 1000f || Vector3.Distance(target, last) > DISTANCE_CAP) {
        last = target;
        return target;
      }

      float fps = 1 / delta;
      float newFactor = Mathf.Lerp(
        POSITION_SCALE.x, POSITION_SCALE.y, (fps - FPS_RANGE.x) / (FPS_RANGE.y - FPS_RANGE.x)
      );
      positionFactor = Mathf.Lerp(positionFactor, newFactor, Time.smoothDeltaTime * FACTOR_SCALE);

      last = Vector3.Lerp(last, target, Time.smoothDeltaTime * positionFactor);
      return last;
    }

    /// <summary>
    /// Smooth a Quaternion given a reference to its last state and a delta time.
    /// Generally this is called within an update function.
    /// </summary>
    /// <param name="target"></param>
    /// <param name="last"></param>
    /// <param name="delta"></param>
    /// <returns>A smoothed version of the input.</returns>
    public Quaternion Smooth(Quaternion target, ref Quaternion last, float delta) {
      if (delta > TIMEOUT / 1000f || Quaternion.Dot(target, last) < DOT_MIN) {
        last = target;
        return target;
      }

      float fps = 1 / delta;
      float newFactor = Mathf.Lerp(
        ROTATION_SCALE.x, ROTATION_SCALE.y, (fps - FPS_RANGE.x) / (FPS_RANGE.y - FPS_RANGE.x)
      );
      rotationFactor = Mathf.Lerp(rotationFactor, newFactor, Time.smoothDeltaTime * FACTOR_SCALE);

      last = Quaternion.Slerp(last, target, Time.smoothDeltaTime * rotationFactor);
      return last;
    }
  }
}
