using System;
using UnityEngine;
namespace Holojam {
     public class TrackedObject : MonoBehaviour {

          public LiveObjectTag liveObjectTag;

          protected MasterStream masterStream;
          protected bool isTracked;
          protected Vector3 trackedPosition;
          protected Quaternion trackedRotation;

          public bool IsTracked {
               get {
                    return isTracked;
               }
          }

          private void Start() {
               masterStream = MasterStream.Instance;
          }

          protected virtual void Update() {
               this.UpdateTracking();
               if (this.isTracked) {
                    this.transform.localPosition = trackedPosition;
                    this.transform.localRotation = trackedRotation;
               }
          }

          protected void UpdateTracking() {
               Vector3 position;
               Quaternion rotation;

               if (masterStream.GetPosition(liveObjectTag, out position) && masterStream.GetRotation(liveObjectTag, out rotation)) {
                    this.isTracked = true;
                    this.trackedPosition = position;
                    this.trackedRotation = rotation;

               } else {
                    this.isTracked = false;
               }
          }
     }
}