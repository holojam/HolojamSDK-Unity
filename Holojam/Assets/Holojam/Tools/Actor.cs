//Actor.cs
//Created by Aaron C Gaudette on 23.06.16
//Umbrella class for accessing player (headset user) data in a generic manner,
//decoupled from the build process and VR camera setup.
//This barebones base-class implementation is sufficient for tracking a head--
//extend for more complex use-cases.

using UnityEngine;
using System.Collections.Generic;
using Holojam.Utility;

namespace Holojam.Tools{
   public class Actor : Trackable{
      public static List<Actor> instances = new List<Actor>();
      public static Dictionary<string,Actor> localInstances = new Dictionary<string,Actor>();
      void OnEnable(){instances.Add(this);}
      void OnDisable(){instances.Remove(this);}

      public int index;
      public bool isBuild{get{return BuildManager.BUILD_ACTOR==this;}}
      public bool isLocal{get{return localInstances.ContainsKey(brand);}}

      protected const Utility.Palette.Colors DEFAULT_COLOR = Utility.Palette.Colors.SEA_FOAM;
      public Color debugColor = Utility.Palette.Select(DEFAULT_COLOR);

      const float dropInterval = 1;
      bool lastTracked = false; float lastTime;

      public override string labelField{get{return Network.Canon.IndexToLabel(index);}}

      void UpdateData(){
         //Update local instances dictionary
         bool local = view.scope==Network.Client.SEND_SCOPE || string.IsNullOrEmpty(view.scope);
         if(local && !localInstances.ContainsKey(brand))
            localInstances[brand] = this;
         else if(!local && localInstances.ContainsKey(brand))
            localInstances.Remove(brand);

         //Call fade events
         if(Time.time>lastTime+dropInterval){
            if(view.tracked!=lastTracked){
               if(view.tracked)FadeIn();
               else FadeOut();
            }
            lastTracked = view.tracked;
            lastTime = Time.time;
         }
      }

      //Override these in derived classes for custom unique implementation

      protected override void Update(){
         base.Update(); //See Controller.cs for details
         UpdateData(); //Mandatory call
      }
      protected override void UpdateTracking(){
         base.UpdateTracking(); //See Trackable.cs for details
      }
      //Events called on join/loss
      protected virtual void FadeIn(){}
      protected virtual void FadeOut(){}

      //These generic accessors enable reliable Actor information to be obtained from outside the class.
      //They should always reference assigned data (e.g. transform.position), not source (raw) data,
      //so that the changes made in update are reflected
      public virtual Vector3 center{
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
         get{return rawRotation;}
      }
      //TODO

      //Useful derived accessors
      public Vector3 look{get{return orientation*Vector3.forward;}}
      public Vector3 up{get{return orientation*Vector3.up;}}
      public Vector3 left{get{return orientation*Vector3.left;}}

      //Useful (goggles) visualization
      void OnDrawGizmos(){
         DrawGizmoGhost();
         Gizmos.color = debugColor;
         Vector3 offset = center+look*0.0f;
         Utility.Drawer.Circle(offset+left*0.035f,look,up,0.03f);
         Utility.Drawer.Circle(offset-left*0.035f,look,up,0.03f);
         //Reference forward vector
         Gizmos.DrawRay(offset,look);
      }
      void OnDrawGizmosSelected(){} //Override Trackable implementation
   }
}
