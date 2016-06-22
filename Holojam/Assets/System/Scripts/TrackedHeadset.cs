//using System;
//using UnityEngine;
//namespace Holojam {
//     public class TrackedHeadset : TrackedObject {

//          protected override void Update() {
//               base.Update();

//               Quaternion goalOrientation = UnityEngine.VR.InputTracking.GetLocalRotation(UnityEngine.VR.VRNode.CenterEye);
//               if (this.IsTracked) {
//                    goalOrientation = trackedRotation * Quaternion.Inverse(goalOrientation);
//               } 

//               this.transform.rotation = Quaternion.Slerp(this.transform.rotation, goalOrientation, Mathf.Clamp(Time.deltaTime, 0, 1));
//          }
//     }
//}

