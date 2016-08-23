//ActorController.cs
//Created by Aaron C Gaudette on 05.07.16
//Example Actor extension

using UnityEngine;

public class ActorController : Holojam.Tools.Actor{
   public Transform head;

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
}