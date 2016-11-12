//ActorController.cs
//Created by Aaron C Gaudette on 05.07.16
//Example Actor extension

using UnityEngine;
using System.Collections;

public class ActorController : Holojam.Tools.Actor{
   public Transform head;
   public Transform animatedEyes;

   const float BLINK_TIME = 0.085f;
   readonly Vector2 BLINK_DELAY = new Vector2(1,11);

   float blinkDelay = 0, lastBlink = 0;

   protected override void Update(){
      UpdateView();

      if(!Application.isPlaying)return;

      //Update overridden to add this line
      if(actorManager.runtimeIndexing)ApplyMotif();

      UpdateTracking();
   }
   protected override void UpdateTracking(){
      if(view.IsTracked){
         transform.position = trackedPosition;

         //This example type uses a separate transform for rotation (a head) instead of itself
         if(head!=null){
            head.localPosition = Vector3.zero;
            head.rotation = trackedRotation;
         }
         else Debug.LogWarning("ActorController: No head found for "+gameObject.name);
      }
   }
   //The orientation accessor matches the rotation assignment above
   public override Quaternion orientation{
      get{return head!=null?head.rotation:Quaternion.identity;}
   }

   //Assign color of geometry with Motif tag
   void Start(){ApplyMotif();}

   void ApplyMotif(){
      if(Application.isPlaying)
         foreach(Renderer r in GetComponentsInChildren<Renderer>(true))
            if(r.gameObject.tag=="Motif")r.material.color = motif;
   }

   void LateUpdate(){
      //Blink
      if(animatedEyes!=null && Time.time>lastBlink+blinkDelay)Blink();
   }
   //Blink
   void Blink(){
      StartCoroutine(ToggleEyes(animatedEyes.localScale));
      blinkDelay = Random.Range(BLINK_DELAY.x,BLINK_DELAY.y);
      lastBlink = Time.time;
   }
   IEnumerator ToggleEyes(Vector3 initialScale, bool close = true){
      float initialTime = Time.time;
      Vector3 shut = new Vector3(initialScale.x,0,initialScale.z);
      while(close?animatedEyes.localScale.y>0:animatedEyes.localScale.y<initialScale.y){
         animatedEyes.localScale = Vector3.Lerp(
            close?initialScale:shut,close?shut:initialScale,
            (Time.time-initialTime)/BLINK_TIME
         );
         yield return null;
      }
      if(close)StartCoroutine(ToggleEyes(initialScale,false));
   }
}