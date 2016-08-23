//PairTarget.cs
//Created by Aaron C Gaudette on 20.08.16

using UnityEngine;
using System;
using System.Collections.Generic;

namespace Holojam.Tools{
   [ExecuteInEditMode]
   [RequireComponent(typeof(SphereCollider)), RequireComponent(typeof(Rigidbody))]
   public class PairTarget : MonoBehaviour{
      //Transform to extend from for pairable detection
      public Transform pivot; //Assign an actor for specific support--see below
      //Pairing parameters
      public float diameter = 0.16f;
      public float distance = 0.18f;

      [Space(8)]
      //Allow or disallow specific pairable types
      public string[] mask = new string[0];
      public enum MaskType{WHITELIST,BLACKLIST};
      public MaskType maskType = MaskType.BLACKLIST;

      //Cap the number of a specific type of pairable (0 = unlimited)
      //Ideally would be implemented using some kind of Unity serializable dictionary
      [System.Serializable]
      public class TypeCap{
         public string type;
         public int cap = 0;
         public TypeCap(string type, int cap){
            this.type=type;
            this.cap=cap;
         }
      }
      public TypeCap[] typeCaps = {new TypeCap("Grip",2)};

      [System.Serializable]
      public class PairingZoneDisplay{
         public Renderer renderer;
         public bool scale = true;
         //Renders when any pairable is in the zone regardless of pivotActor
         public bool forceOn = false;
      }
      public PairingZoneDisplay zoneDisplay;

      [Header("Read-Only:")]
      public List<Pairable> paired = new List<Pairable>();
      Dictionary<Pairable,float> buffer = new Dictionary<Pairable,float>();

      //For color, accurate transform, and pairing zone display per-build
      [HideInInspector] public Actor pivotActor;

      void OnValidate(){Update();}
      void UpdatePairingZone(){
         if(Application.isPlaying)return;

         SphereCollider c = GetComponent<SphereCollider>();
         c.center = new Vector3(0,0,0);
         c.radius = diameter*0.5f;
         c.isTrigger = true;

         Rigidbody r = GetComponent<Rigidbody>();
         r.useGravity = false;
         r.isKinematic = true;
         r.constraints = RigidbodyConstraints.None;
      }
      void Update(){
         UpdatePairingZone();

         if(pivot==null){
            Debug.LogWarning("PairTarget: No pivot!",this);
            return;
         }
         if(pivotActor==null)
            pivotActor = pivot.GetComponent<Actor>();

         //Use actor data if set
         Vector3 position = pivotActor==null?pivot.position:pivotActor.eyes;
         Vector3 forward = pivotActor==null?pivot.forward:pivotActor.look;
         transform.position = position+forward*distance;

         //Render pairing zone
         if(zoneDisplay.renderer!=null){
            zoneDisplay.renderer.enabled = buffer.Count>0 &&
               (zoneDisplay.forceOn || (pivotActor!=null && pivotActor.isBuild));
            zoneDisplay.renderer.transform.localScale = new Vector3(diameter,diameter,diameter);
         }
         else Debug.LogWarning("PairTarget: No pairing zone display!",this);

         if(!Application.isPlaying)return;

         List<Pairable> keys = new List<Pairable>(buffer.Keys);

         //Process pairables and pair
         foreach(Pairable p in keys)
            //Validate, but within the buffer--then check timer
            if(ValidatePair(p) && Lifetime(p)>=((p.pairedTo==this)?p.unpairTime:p.pairTime))
               p.Pair(p.pairedTo==this?null:this);

         //Debug
         #if UNITY_EDITOR
         float tick = 0.1f*(Time.time%1);
         foreach(Pairable p in keys){
            bool valid = ValidatePair(p);
            if((p.pairedTo==null || p.pairedTo==this) && valid){
               //Swap color if unpairing
               Color c = pivotActor!=null?pivotActor.motif:Color.white;
               c = p.pairedTo==this?Color.black:c;
               Debug.DrawLine(p.center,position,c);

               //Draw animated axis
               Vector3 progress = Vector3.Lerp(p.center,position,
                  Lifetime(p)/(p.pairedTo==this?p.unpairTime:p.pairTime));
               Debug.DrawLine(
                  progress+Vector3.up*tick,
                  progress-Vector3.up*tick,c
               );
               Debug.DrawLine(
                  progress+Vector3.left*tick,
                  progress-Vector3.left*tick,c
               );
               Debug.DrawLine(
                  progress+Vector3.forward*tick,
                  progress-Vector3.forward*tick,c
               );
            }
            else if(!valid)Debug.DrawLine(p.center,position,Color.gray);
         }
         foreach(Pairable p in paired)
            if(!p.unpairing)
               Debug.DrawLine(p.center,position,
                  pivotActor!=null?pivotActor.motif:Color.white);
         #endif
      }

      void OnTriggerEnter(Collider c){
         Pairable p = c.GetComponent<Pairable>();
         if(p!=null){
            buffer.Add(p,Time.time);
            p.RegisterTarget(this);
         }
      }
      void OnTriggerExit(Collider c){
         Pairable p = c.GetComponent<Pairable>();
         if(p!=null){
            buffer.Remove(p);
            p.DeregisterTarget(this);
         }
      }

      //How long has the pairable been in the pairing zone?
      float Lifetime(Pairable p){return Time.time-buffer[p];}

      public bool ValidatePair(Pairable p){
         bool valid = Array.Exists(mask,s=>s==p.type)==(maskType==MaskType.WHITELIST)
            && (p.pairedTo==this || UnderCap(p.type)) //Cap, but not if unpairing
            && p.CanTarget(this);
         //Reset timer if invalid
         if(!valid)buffer[p]=Time.time;
         return valid;
      }

      //How many pairables of the input type are paired?
      public int PairCount(string type){
         int count = 0;
         foreach(Pairable p in paired)
            if(p.type==type)count++;
         return count;
      }
      //Check if pairable type is under the cap for this target
      public bool UnderCap(string type){
         foreach(TypeCap cap in typeCaps)
            if(cap.type==type)
               return cap.cap==0 || PairCount(type)<cap.cap;
         return true; //If not found, assume unlimited
      }

      //Update paired list and reset timers
      void OnPair(Pairable p){
         buffer[p]=Time.time;
         paired.Add(p);
      }
      void OnUnpair(Pairable p){
         buffer[p]=Time.time;
         paired.Remove(p);
      }
   }
}