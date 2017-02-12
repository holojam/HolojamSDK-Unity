// Viewer.cs
// Created by Holojam Inc. on 07.07.16

using UnityEngine;

namespace Holojam.Tools {
  [ExecuteInEditMode]
  public class Viewer : MonoBehaviour {
    public enum TrackingType { DIRECT, ACTOR };
    public TrackingType trackingType = TrackingType.ACTOR;

    public Converter converter;

    //Get tracking data from actor (recommended coupling), or from the View?
    public Actor actor = null;
    [HideInInspector]
    public Network.View view = null;
    public int index = 1;
    public bool localSpace = false;

    //Update late to catch local space updates
    void LateUpdate() {
      if (converter == null) {
        Debug.LogWarning("Viewer: Converter is null!");
        return;
      }

      if (BuildManager.DEVICE == BuildManager.Device.VIVE)
        return;

      //Flush extra components if necessary
      Network.View[] views = GetComponents<Network.View>();
      if ((view == null && views.Length > 0) || (view != null && (views.Length > 1 || views.Length == 0))) {
        foreach (Network.View v in views) DestroyImmediate(v);
        view = null; //In case the View has been set to a prefab value
      }

      //Automatically add a View component if not using a reference actor
      if (actor == view) {
        view = gameObject.AddComponent<Network.View>() as Network.View;
        view.triples = new Vector3[1];
        view.quads = new Quaternion[1];
      } else if (actor != null && view != null) DestroyImmediate(view);

      if (view != null) {
        view.label = Network.Canon.IndexToLabel(index);
        view.scope = Network.Client.SEND_SCOPE;
      }
      if (!Application.isPlaying) return;

      Vector3 sourcePosition = GetPosition();
      Quaternion sourceRotation = GetRotation();
      bool sourceTracked = GetTracked();

      if (sourceTracked) {
        //TrackingType.DIRECT:
        //Direct raw conversion from Converter (no additional transformation)

        //TrackingType.ACTOR:
        //Loops once through the network (Converter -> Server -> Actor -> Viewer)

        transform.position = sourcePosition;
        if (BuildManager.IsMasterClient())
          transform.rotation = sourceRotation;
        else {
          //Negate IMU
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
      } else if (BuildManager.IsMasterClient()) //Fall back to IMU
        transform.rotation = sourceRotation;

      //Apply local rotation if necessary
      if (actor != null && actor.localSpace && actor.transform.parent != null)
        transform.rotation = actor.transform.parent.rotation * transform.rotation;
      else if (actor == null && localSpace && transform.parent != null)
        transform.rotation = transform.parent.rotation * transform.rotation;
    }

    //Get tracking data from desired source
    Vector3 GetPosition() {
      if (trackingType == TrackingType.DIRECT)
        return converter.OutputPosition;
      else {
        return actor != null ? actor.Center :
        localSpace && transform.parent != null ?
           transform.parent.TransformPoint(view.triples[0]) : view.triples[0];
      }
    }
    Quaternion GetRotation() {
      if (trackingType == TrackingType.DIRECT)
        return converter.OutputRotation;
      else return actor != null ? actor.RawOrientation : view.quads[0];
    }
    bool GetTracked() {
      if (trackingType == TrackingType.DIRECT)
        return converter.HasInput;
      else return actor != null ? actor.view.tracked : view.tracked;
    }
  }
}
