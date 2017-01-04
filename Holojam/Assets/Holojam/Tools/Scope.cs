//Scope.cs
//Created by Aaron C Gaudette on 11.11.16

using UnityEngine;

namespace Holojam.Tools{
   public class Scope : Controller{
      public Vector2 origin;
      [Range(-90,90)] public float angle = 0;

      protected override ProcessDelegate Process{get{return UpdateFrustum;}}

      public float height{get{return view.rawPosition.x;}}
      //public float angle{get{return view.rawPosition.y;}}
      public float stem{get{return view.rawPosition.z;}}

      protected override void UpdateViewLabel(string label){
         view.label = "ExtraData";
         this.label = view.label;
      }
      protected override void UpdateViewScope(string scope){
         view.scope = "Holoscope";
         this.scope = view.scope;
      }
      protected override void UpdateViewSending(bool sending){
         view.sending = false;
         this.sending = view.sending;
      }

      void UpdateFrustum(){
         transform.position = new Vector3(origin.x,height,origin.y);
         transform.rotation = Quaternion.AngleAxis(-angle,Vector3.right);
      }

      public Vector3 Localize(Vector3 position){
         return transform.TransformPoint(position);
      }
      public Quaternion Localize(Quaternion rotation){
         //return rotation;
         return rotation*Quaternion.Inverse(transform.rotation); //
      }
   }
}
