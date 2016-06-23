using System;
using UnityEngine;
namespace Holojam {
     public class TrackedHeadset : TrackedObject {

          protected override void Update() {
               this.UpdateTracking();
               
               Quaternion goalOrientation = UnityEngine.VR.InputTracking.GetLocalRotation(UnityEngine.VR.VRNode.CenterEye);
               if (this.IsTracked) {
                    goalOrientation = trackedRotation * Quaternion.Inverse(goalOrientation);
				this.transform.position = trackedPosition;
               }
			
               this.transform.rotation = Quaternion.Slerp(this.transform.rotation, goalOrientation, Mathf.Clamp(Time.deltaTime, 0, 1));
          }
     }
}