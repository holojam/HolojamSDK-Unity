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
				this.transform.position = trackedPosition;
				this.transform.rotation = trackedRotation;
               }
          }

          protected void UpdateTracking() {
               Vector3 position;
               Quaternion rotation;

               if (masterStream.GetPosition(liveObjectTag, out position) && masterStream.GetRotation(liveObjectTag, out rotation)) {
                    this.isTracked = true;
                    this.trackedPosition = position;
                    this.trackedRotation = rotation;

					//Debug.Log (liveObjectTag + " " + trackedPosition + " " + trackedRotation);

               } else {
                    this.isTracked = false;
               }
          }
     }
}