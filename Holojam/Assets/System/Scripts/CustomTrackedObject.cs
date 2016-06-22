using UnityEngine;
using System.Collections;

namespace Holojam {
     public class CustomTrackedObject : MonoBehaviour {

          public string customTag;


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

               if (masterStream.GetPosition(customTag, out position) && masterStream.GetRotation(customTag, out rotation)) {
                    this.isTracked = true;
                    this.trackedPosition = position;
                    this.trackedRotation = rotation;

               } else {
                    this.isTracked = false;
               }
          }
     }
}

