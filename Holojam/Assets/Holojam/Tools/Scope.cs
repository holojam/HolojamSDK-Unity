//Scope.cs
//Created by Aaron C Gaudette on 11.11.16

using UnityEngine;

namespace Holojam.Tools{
   public class Scope : Controller{
      public Vector2 origin;
      public float height = 1; //Meters
      [Range(-90,90)] public float angle = 0; //Degrees

      protected override ProcessDelegate Process{get{return UpdateFrustum;}}

      public float stem{get{return GetFloat(0);}}

      public override string labelField{get{return "ExtraData";}}
      public override string scopeField{get{return "Holoscope";}}
      public override bool isSending{get{return false;}}

      public override int floatCount{get{return 1;}}

      void UpdateFrustum(){
         transform.position = new Vector3(origin.x,height,origin.y);
         transform.rotation = Quaternion.AngleAxis(-angle,Vector3.right);
      }

      public Vector3 Localize(Vector3 position){
         return transform.TransformPoint(position);
      }
      public Quaternion Localize(Quaternion rotation){
         return Quaternion.Inverse(transform.rotation)*rotation;
      }
   }
}
