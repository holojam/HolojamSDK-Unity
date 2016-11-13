//ViewDebugger.cs
//Created by Aaron C Gaudette on 13.07.16

using UnityEngine;

namespace Holojam.Tools{
   public class ViewDebugger : Controller{
      void Reset(){
         scope = "Scope";
      }
      protected override ProcessDelegate Process{get{return UpdateData;}}

      public Vector3 rawPosition;
      public Quaternion rawRotation;
      public int bits;
      public string blob;

      protected override void UpdateViewScope(string scope){
         view.scope = scope;
      }

      void UpdateData(){
         rawPosition = view.rawPosition;
         rawRotation = view.rawRotation;
         bits = view.bits;
         blob = view.blob;
      }
   }
}