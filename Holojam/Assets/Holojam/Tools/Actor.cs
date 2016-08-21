//Actor.cs
//Created by Aaron C Gaudette on 23.06.16
//Umbrella class for accessing player (headset user) data in a generic manner,
//decoupled from the build process and VR camera setup.
//This barebones base-class implementation is sufficient for tracking a head--
//extend for more complex use-cases.

using UnityEngine;
using Holojam.Network;

namespace Holojam.Tools{
   public class Actor : Trackable{
      public string handle = "Actor";
      public Color motif = Color.white; //Useful color identifier, optional for rendering
      void Reset(){trackingTag = Motive.Tag.HEADSET1;}
      public GameObject mask; //This object is disabled for build actors by the manager

      public int index{get{return (int)trackingTag;}}
      ActorManager manager;
      public ActorManager actorManager{
         get{
            if(manager==null && transform.parent!=null)
               manager = transform.parent.GetComponent<ActorManager>();
            return manager;
         }
      }
      public bool isBuild{get{
         return actorManager!=null && actorManager.buildActor==this;
      }}

      //Override these in derived classes for custom unique implementation

      protected override void Update(){
         base.Update(); //See Trackable.cs for details
      }
      protected override void UpdateTracking(){
         base.UpdateTracking(); //See Trackable.cs for details
      }
      //These generic accessors enable reliable Actor information to be obtained from outside the class.
      //They should always reference assigned data (e.g. transform.position), not source (raw) data
      public virtual Vector3 eyes{
         get{return transform.position;}
      }
      public virtual Quaternion orientation{
         get{return transform.rotation;}
      }
      //This accessor dictates where each user is looking in their headset. Override for unique
      //edge cases--when you are manually augmenting the actor rotation or when you want
      //the user's look direction to differ from what the actor is broadcasting (not recommended)
      public virtual Quaternion rawOrientation{
         //Be careful not to map rotation to anything other than the raw data
         //(the user's actual head movement) unless you absolutely know what you're doing.
         //The Viewer (VR camera) uses a custom tracking algorithm and relies on the
         //orientation accessor below to provide absolute truth.
         //Alternatively, use the Viewer's OPTICAL tracking type if you want the headset's
         //rotation to match this value exactly
         get{return view.RawRotation;}
      }

      //Useful derived accessors
      public Vector3 look{get{return orientation*Vector3.forward;}}
      public Vector3 up{get{return orientation*Vector3.up;}}
      public Vector3 left{get{return orientation*Vector3.left;}}

      //Useful (goggles) visualization for edge of GearVR headset
      void OnDrawGizmos(){
         DrawGizmoGhost();
         Gizmos.color = motif;
         Vector3 offset = eyes+look*0.015f;
         Drawer.Circle(offset+left*0.035f,look,up,0.03f);
         Drawer.Circle(offset-left*0.035f,look,up,0.03f);
         //Reference forward vector
         Gizmos.DrawRay(offset,look);
      }
      void OnDrawGizmosSelected(){}
   }
}