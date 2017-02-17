// Converter.cs
// Created by Holojam Inc. on 11.11.16

#define SMOOTH

using UnityEngine;
using Holojam.Tools;

namespace Holojam.Martian {

  /// <summary>
  /// Converts raw Martian (Holoscope) data to a stable transform with
  /// sensor fusion and smoothing.
  /// </summary>
  public class Converter : MonoBehaviour {

    /// <summary>
    /// Serializable class container for positional smoothing variables.
    /// </summary>
    [System.Serializable]
    public class Smoothing {
      public float cap, pow;
      public Smoothing(float cap, float pow) {
        this.cap = cap;
        this.pow = pow;
      }
    }
    readonly Smoothing XY_SMOOTHING = new Smoothing(.05f, 1.1f);
    readonly Smoothing Z_SMOOTHING = new Smoothing(.15f, 2);
    const float R_SMOOTHING = .12f;

    [SerializeField] Tracker extraData;
    [SerializeField] Viewer viewer;

    public enum DebugMode { NONE, POSITION, REMOTE }
    /// <summary>
    /// Debug mode for testing the Martian in the editor.
    /// Use POSITION when you only have the device, and REMOTE
    /// when you've built the Holojam Remote app to your phone.
    /// </summary>
    public DebugMode debugMode = DebugMode.NONE;

    #if SMOOTH
      Vector3 lastLeft = Vector3.zero, lastRight = Vector3.zero;
    #endif

    /// <summary>
    /// Input and output Controllers for conversion.
    /// </summary>
    Martian input;
    Relay output;

    Transform imu;
    Remote remote;
    Quaternion raw, correction, correctionTarget = Quaternion.identity;

    public bool HasInput { get { return input.Tracked; } }

    public Vector3 OutputPosition { get { return output.Position; } }
    public Quaternion OutputRotation { get { return output.Rotation; } }

    void Awake() {
      if (BuildManager.DEVICE == BuildManager.Device.VIVE)
        return;

      // Ignore debug flags on builds
      if (!BuildManager.IsMasterClient())
        debugMode = DebugMode.NONE;

      if (extraData == null) {
        Debug.LogWarning("Converter: Extra data reference is null!");
        return;
      }
      if (BuildManager.IsMasterClient() && debugMode == DebugMode.NONE)
        return;

      imu = viewer.transform.GetChild(0);
      if (debugMode == DebugMode.REMOTE)
        remote = gameObject.AddComponent<Remote>() as Remote;

      input = gameObject.AddComponent<Martian>() as Martian;
      output = gameObject.AddComponent<Relay>() as Relay;
    }

    /// <summary>
    /// Sensor fusion and smoothing between the input and output Controllers.
    /// </summary>
    void Update() {
      if (BuildManager.DEVICE == BuildManager.Device.VIVE)
        return;

      // Editor debugging
      if (BuildManager.IsMasterClient()) {
        if (debugMode == DebugMode.POSITION) {
          #if SMOOTH
            output.Position = extraData.Localize((
               SmoothPosition(input.Left, ref lastLeft) +
               SmoothPosition(input.Right, ref lastRight)) * .5f
            );
          #else
            output.Position = extraData.Localize(.5f * (input.Left + input.Right));
          #endif
          return;
        } else if (debugMode == DebugMode.NONE) return;
      }

      #if SMOOTH
        Vector3 left = SmoothPosition(input.Left, ref lastLeft);
        Vector3 right = SmoothPosition(input.Right, ref lastRight);
      #else
        Vector3 left = input.Left, right = input.Right;
      #endif
      Vector3 inputPosition = .5f * (left + right);

      // Get IMU data
      if (debugMode == DebugMode.REMOTE)
        raw = remote.Imu;
      else switch (BuildManager.DEVICE) {
          case BuildManager.Device.CARDBOARD:
          raw = imu.localRotation;
          break;
          case BuildManager.Device.DAYDREAM:
          raw = UnityEngine.VR.InputTracking.GetLocalRotation(
             UnityEngine.VR.VRNode.CenterEye
          );
          break;
        }

      // Update target if tracked
      if (HasInput) {
        // Read in secondary vector
        Vector3 nbar = (right - left).normalized;

        Vector3 imuUp = raw * Vector3.up;
        Vector3 imuForward = raw * Vector3.forward;
        Vector3 imuRight = raw * Vector3.right;

        // Compare orientations relative to gravity
        Quaternion difference = Quaternion.LookRotation(-nbar, Vector3.up)
           * Quaternion.Inverse(Quaternion.LookRotation(imuRight, Vector3.up));

        Vector3 newForward = difference * imuForward;
        Vector3 newUp = difference * imuUp;

        // Ideal rotation
        Quaternion target = Quaternion.LookRotation(newForward, newUp);
        correctionTarget = target * Quaternion.Inverse(raw);
      }

      #if SMOOTH
        // Lazily interpolate correction (only has to be a baseline, not immediate)
        correction = Quaternion.Slerp(
          correction, correctionTarget, Time.deltaTime * R_SMOOTHING
        );
      #else
        correction = correctionTarget;
      #endif

      // Update output
      output.Rotation = extraData.Localize(correction * raw);
      output.Position = extraData.Localize(
         //inputPosition - outputRotation * Vector3.up * extraData.Stem
         inputPosition
      );
    }

    /// <summary>
    /// Component smoothing function for reducing noise while minimizing perceived latency in the
    /// signal.
    /// </summary>
    Vector3 SmoothPosition(Vector3 target, ref Vector3 last) {
      Vector2 xyLast = new Vector2(last.x, last.y);
      Vector2 xyTarget = new Vector2(target.x, target.y);

      Vector2 xy = Vector2.Lerp(xyLast, xyTarget, Mathf.Pow(
         Mathf.Min(1, (xyLast - xyTarget).magnitude / XY_SMOOTHING.cap), XY_SMOOTHING.pow
      ));
      float z = Mathf.Lerp(last.z, target.z, Mathf.Pow(
         Mathf.Min(1, Mathf.Abs(last.z - target.z) / Z_SMOOTHING.cap), Z_SMOOTHING.pow
      ));

      target = new Vector3(xy.x, xy.y, z);

      last = target;
      return target;
    }
  }
}
