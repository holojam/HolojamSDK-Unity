//Pairable.cs
//Created by Aaron C Gaudette on 24.06.16

using UnityEngine;
using System;
using System.Collections.Generic;
using Holojam.Network;

namespace Holojam.Tools{
   public class Pairable : Trackable{
      public string type = "Grip";
      void Reset(){trackingTag=Motive.Tag.LEFTHAND1;}
      //Offset to center of model, not pivot point
      public Vector3 centerOffset = Vector3.zero;
      public Vector3 center{get{return transform.TransformPoint(centerOffset);}}

      //Pairing parameters
      public float pairTime = 1.3f;
      public float unpairTime = 1.6f;
      public float cooldown = 1.5f;

      public PairTarget pairedTo = null;
      float lastPaired = 0;

      List<PairTarget> targets = new List<PairTarget>();
      public void RegisterTarget(PairTarget target){targets.Add(target);}
      public void DeregisterTarget(PairTarget target){targets.Remove(target);}

      public bool paired{get{return pairedTo!=null;}}
      public bool pairing{get{
         return targets.FindAll(t=>t.ValidatePair(this)).Count>0 && !paired;
      }}
      public bool unpairing{get{
         return targets.FindAll(t=>t.ValidatePair(this)).Count>0 && paired;
      }}

      //Override to modify targeting validation
      public virtual bool CanTarget(PairTarget target){
         return (!paired || pairedTo==target) //Don't pair with another target's pairable
            && Time.time-lastPaired>cooldown;
      }

      public void Pair(PairTarget target){
         //Notify target game object
         if(target!=null)
            target.SendMessage("OnPair",this,SendMessageOptions.DontRequireReceiver);
         else pairedTo.SendMessage("OnUnpair",this,SendMessageOptions.DontRequireReceiver);

         pairedTo = target;
         lastPaired = Time.time;
         RenderPair();

         /*
         Debug.Log(target!=null?
            "Pairable: "+gameObject.name+" ("+trackingTag+") paired with \""+
               target.pivot.gameObject.name+"\"":
            "Pairable: "+gameObject.name+" ("+trackingTag+") unpaired",
            this
         );
         */
      }

      protected override void Update(){
         base.Update();
         RenderPairing();
      }

      //Override these in derived classes
      protected virtual void RenderPair(){
         //By default, changes color
         Color c = pairedTo==null?Color.gray:
            pairedTo.pivotActor!=null?pairedTo.pivotActor.motif:Color.white;
         foreach(Renderer r in GetComponentsInChildren<Renderer>()){
            if(r.gameObject.tag=="Motif")r.material.color=c;
            r.enabled=true;
         }
      }
      protected virtual void RenderPairing(){
         //Blink effect
         foreach(Renderer r in GetComponentsInChildren<Renderer>())
            r.enabled=(pairing || unpairing)?Time.time%0.2f>0.1f:true;
      }

      void OnDrawGizmosSelected(){
         //Pivot
         Gizmos.color=Color.gray;
         Gizmos.DrawLine(transform.position-0.01f*Vector3.up,transform.position+0.01f*Vector3.up);
         Gizmos.DrawLine(transform.position-0.01f*Vector3.left,transform.position+0.01f*Vector3.left);
         Gizmos.DrawLine(transform.position-0.01f*Vector3.forward,transform.position+0.01f*Vector3.forward);
         //Offset
         Gizmos.color=pairedTo==null?Color.gray:
            pairedTo.pivotActor!=null?pairedTo.pivotActor.motif:Color.white;
         Gizmos.DrawWireSphere(center,0.06f);
      }
   }
}