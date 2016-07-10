using System;
using UnityEngine;
using Holojam.Network;

namespace Holojam {
	[Obsolete("MasterStream is deprecated. Please use HolojamNetwork.")]
	public class TrackedObject : MonoBehaviour {

          public Motive.Tag liveObjectTag;


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
               } else {
                    this.isTracked = false;
               }
          }
     }
}