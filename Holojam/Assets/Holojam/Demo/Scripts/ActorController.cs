//ActorController.cs
//Created by Aaron C Gaudette on 05.07.16
//Example Actor extension

using UnityEngine;
using System.Collections;

public class ActorController : Holojam.Tools.Actor{
   const float BLINK_TIME = 0.085f;
   readonly Vector2 BLINK_DELAY = new Vector2(1,11);

   public Transform head;
   public GameObject mask; //Disabled for build actors

   public Color motif = Holojam.Utility.Palette.Select(DEFAULT_COLOR);
   public Transform animatedEyes;
   public Material skinMaterial;

   float blinkDelay = 0, lastBlink = 0;

   protected override void UpdateTracking(){
      if(view.tracked){
         transform.position = trackedPosition;

         //This example type uses a separate transform for rotation (a head) instead of itself
         if(head!=null){
            head.localPosition = Vector3.zero;
            head.rotation = trackedRotation;
         }
         else Debug.LogWarning("ActorController: No head found for "+gameObject.name,this);
      }

      //Toggle mask
      if(mask!=null)
         mask.SetActive(!isBuild);
   }
   //The orientation accessor matches the rotation assignment above
   public override Quaternion orientation{
      get{return head!=null?head.rotation:Quaternion.identity;}
   }

   //Assign color and skin material
   void Start(){ApplyMotif();}

   void ApplyMotif(){
      debugColor = motif;
      if(Application.isPlaying)
         foreach(Renderer r in GetComponentsInChildren<Renderer>(true)){
            if(r.gameObject.tag=="Motif")r.material.color = motif;
            if(r.gameObject.tag=="Skin")r.material = skinMaterial;
         }
   }

   //Blink
   void LateUpdate(){
      if(animatedEyes!=null && Time.time>lastBlink+blinkDelay)Blink();
   }
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
