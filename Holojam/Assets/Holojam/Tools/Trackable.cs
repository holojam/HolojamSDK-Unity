//Trackable.cs
//Created by Aaron C Gaudette on 09.07.16
//Base class for trackable entities

using UnityEngine;
using Holojam.Network;

namespace Holojam.Tools{
   public class Trackable : Controller{
      public string label = "Trackable";
      public string scope = "";
      public bool localSpace = false;

      //Allocation
      public override int tripleCount{get{return 1;}}
      public override int quadCount{get{return 1;}}
      //Proxies
      public Vector3 rawPosition{
         get{return GetTriple(0);}
         set{SetTriple(0,value);}
      }
      public Quaternion rawRotation{
         get{return GetQuad(0);}
         set{SetQuad(0,value);}
      }

      //Accessors in case modification needs to be made to the raw data (like smoothing)
      public Vector3 trackedPosition{get{
         return localSpace && transform.parent!=null?
            transform.parent.TransformPoint(rawPosition):rawPosition;
      }}
      public Quaternion trackedRotation{get{
         return localSpace && transform.parent!=null?
            transform.parent.rotation*rawRotation:rawRotation;
      }}

      protected override ProcessDelegate Process{get{return UpdateTracking;}}

      public override string labelField{get{return label;}}
      public override string scopeField{get{return scope;}}
      public override bool isSending{get{return false;}}

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
            rawPosition-0.03f*Vector3.left,
            rawPosition+0.03f*Vector3.left
         );
         Gizmos.DrawLine(
            rawPosition-0.03f*Vector3.forward,
            rawPosition+0.03f*Vector3.forward
         );
         Gizmos.DrawLine(rawPosition-0.03f*Vector3.up,rawPosition+0.03f*Vector3.up);  
      }
   }
}
