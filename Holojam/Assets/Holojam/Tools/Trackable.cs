//Trackable.cs
//Created by Aaron C Gaudette on 09.07.16
//Base class for trackable entities

using UnityEngine;
using Holojam.Network;

namespace Holojam.Tools{
   [ExecuteInEditMode, RequireComponent(typeof(HolojamView))]
   public class Trackable : MonoBehaviour{
      public Motive.Tag trackingTag = Motive.Tag.BOX1;
      public bool localSpace = false;
   
      //Accessors in case modification needs to be made to the raw data (like smoothing)
      public Vector3 trackedPosition{get{
         return localSpace && transform.parent!=null?
            transform.parent.TransformPoint(view.RawPosition): view.RawPosition;
      }}
      public Quaternion trackedRotation{get{
         return localSpace && transform.parent!=null?
            transform.parent.rotation*view.RawRotation : view.RawRotation;
      }}
   
      //Manage view
      public HolojamView view{get{
         if(holojamView==null)holojamView = GetComponent<HolojamView>();
         return holojamView;
      }}
      HolojamView holojamView = null;

      protected void UpdateView(){
         view.Label = Motive.GetName(trackingTag);
         view.IsMine = false;
      }

      //Override these in derived classes
      protected virtual void Update(){
         UpdateView(); //Mandatory initialization call

         //Optional check--you probably don't want to run this code in edit mode
         if(!Application.isPlaying)return;

         UpdateTracking();
      }
      protected virtual void UpdateTracking(){
         //By default, assigns position and rotation injectively
         if(view.IsTracked){
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
         Drawer.Circle(transform.position,Vector3.up,Vector3.forward,0.18f);
         Gizmos.DrawLine(transform.position-0.03f*Vector3.left,transform.position+0.03f*Vector3.left);
         Gizmos.DrawLine(transform.position-0.03f*Vector3.forward,transform.position+0.03f*Vector3.forward);
         //Forward
         Gizmos.DrawRay(transform.position,transform.forward*0.18f);
      }
      //Draw ghost (in world space) if in local space
      protected void DrawGizmoGhost(){
         if(!localSpace || transform.parent==null)return;
         Gizmos.color = Color.gray;
         Gizmos.DrawLine(view.RawPosition-0.03f*Vector3.left,view.RawPosition+0.03f*Vector3.left);
         Gizmos.DrawLine(view.RawPosition-0.03f*Vector3.forward,view.RawPosition+0.03f*Vector3.forward);
         Gizmos.DrawLine(view.RawPosition-0.03f*Vector3.up,view.RawPosition+0.03f*Vector3.up);  
      }
   }
}