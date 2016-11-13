//Trackable.cs
//Created by Aaron C Gaudette on 09.07.16
//Base class for trackable entities

using UnityEngine;
using Holojam.Network;

namespace Holojam.Tools{
   public class Trackable : Controller{
      public bool localSpace = false;

      //Accessors in case modification needs to be made to the raw data (like smoothing)
      public Vector3 trackedPosition{get{
         return localSpace && transform.parent!=null?
            transform.parent.TransformPoint(view.rawPosition): view.rawPosition;
      }}
      public Quaternion trackedRotation{get{
         return localSpace && transform.parent!=null?
            transform.parent.rotation*view.rawRotation : view.rawRotation;
      }}

      protected override ProcessDelegate Process{get{return UpdateTracking;}}

      protected override void UpdateViewSending(bool sending){
         view.sending = false;
         this.sending = view.sending;
      }

      //Override in derived classes
      protected virtual void UpdateTracking(){
         //By default, assigns position and rotation injectively
         if(view.tracked){
            transform.position = trackedPosition;
            transform.rotation = trackedRotation;
         }
         //Untracked maintains last known position and rotation
      }

      void OnDrawGizmos(){
         DrawGizmoGhost();
      }
      void OnDrawGizmosSelected(){
         Gizmos.color = Color.gray;
         //Pivot
         Utility.Drawer.Circle(transform.position,Vector3.up,Vector3.forward,0.18f);
         Gizmos.DrawLine(transform.position-0.03f*Vector3.left,transform.position+0.03f*Vector3.left);
         Gizmos.DrawLine(transform.position-0.03f*Vector3.forward,transform.position+0.03f*Vector3.forward);
         //Forward
         Gizmos.DrawRay(transform.position,transform.forward*0.18f);
      }
      //Draw ghost (in world space) if in local space
      protected void DrawGizmoGhost(){
         if(!localSpace || transform.parent==null)return;
         Gizmos.color = Color.gray;
         Gizmos.DrawLine(
            view.rawPosition-0.03f*Vector3.left,
            view.rawPosition+0.03f*Vector3.left
         );
         Gizmos.DrawLine(
            view.rawPosition-0.03f*Vector3.forward,
            view.rawPosition+0.03f*Vector3.forward
         );
         Gizmos.DrawLine(view.rawPosition-0.03f*Vector3.up,view.rawPosition+0.03f*Vector3.up);  
      }
   }
}
