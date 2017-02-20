// Viewer.cs
// Created by Holojam Inc. on 07.07.16

using UnityEngine;
using Holojam.Tools;

namespace Holojam.Martian {

  /// <summary>
  /// Core class for properly orienting the render camera to the build user.
  /// Operates behind the scenes for the most part, making sure there are no
  /// conflicts between the tracking system (position, rotation) and the phone.
  /// </summary>
  [ExecuteInEditMode]
  public class Viewer : MonoBehaviour {

    public enum TrackingType { DIRECT, ACTOR };

    /// <summary>
    /// The DIRECT tracking type sources data directly from Converter.
    /// The ACTOR tracking type sources data from the associated generic Actor, and
    /// therefore requires an extra loop through the network.
    /// </summary>
    public TrackingType trackingType = TrackingType.DIRECT;

    public Converter converter;

    /// <summary>
    /// Target Actor for tracking data.
    /// </summary>
    [HideInInspector] public Actor actor = null;

    // Update late to catch local space updates
    void LateUpdate() {
      actor = BuildManager.BUILD_ACTOR;

      if (converter == null) {
        Debug.LogWarning("Viewer: Converter is null!");
        return;
      }

      if (BuildManager.DEVICE == BuildManager.Device.VIVE)
        return;

      if (!Application.isPlaying) return;

      Vector3 sourcePosition = GetPosition();
      Quaternion sourceRotation = GetRotation();
      bool sourceTracked = GetTracked();

      if (sourceTracked) {
        // TrackingType.DIRECT:
        // Direct raw conversion from Converter (no additional transformation)

        // TrackingType.ACTOR:
        // Loops once through the network (Converter -> Server -> Actor -> Viewer)

        transform.position = sourcePosition;
        if (BuildManager.IsMasterClient())
          transform.rotation = sourceRotation;
        else {
          // Negate IMU
          Quaternion raw = Quaternion.identity;
          switch (BuildManager.DEVICE) {
            case BuildManager.Device.CARDBOARD:
            raw = transform.GetChild(0).localRotation;
            break;
            case BuildManager.Device.DAYDREAM:
            raw = UnityEngine.VR.InputTracking.GetLocalRotation(
               UnityEngine.VR.VRNode.CenterEye
            );
            break;
          }
          sourceRotation *= Quaternion.Inverse(raw);
          transform.rotation = sourceRotation;
        }
      } else if (BuildManager.IsMasterClient()) // Fall back to IMU
        transform.rotation = sourceRotation;

      // Apply local rotation if necessary
      if (actor != null && actor.LocalSpace && actor.transform.parent != null)
        transform.rotation = actor.transform.parent.rotation * transform.rotation;
    }

    // Is it possible to get data directly from the Converter?
    bool CanGetDirect {
      get {
        return trackingType == TrackingType.DIRECT
          && converter.debugMode != Converter.DebugMode.NONE == BuildManager.IsMasterClient();
      }
    }

    // Get tracking data from desired source

    Vector3 GetPosition() {
      if (CanGetDirect)
        return converter.OutputPosition;
      else
        return actor != null ? actor.Center : Vector3.zero;
    }

    Quaternion GetRotation() {
      if (CanGetDirect)
        return converter.OutputRotation;
      else return actor != null ? actor.RawOrientation : Quaternion.identity;
    }

    bool GetTracked() {
      if (CanGetDirect)
        return converter.HasInput;
      else return actor != null ? actor.Tracked : false;
    }
  }
}
