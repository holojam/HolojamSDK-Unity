//Synchronizable.cs
//Created by Aaron C Gaudette on 11.07.16

using UnityEngine;

namespace Holojam.Tools{
   public abstract class Synchronizable : Controller{
      public string label = "Synchronizable";
      public string scope = "";
      public bool sending = true;
      public bool useMasterPC = false;

      protected override ProcessDelegate Process{get{return Sync;}}

      public override string labelField{get{return label;}}
      public override string scopeField{get{return scope;}}
      public override bool isSending{get{
         return sending && (BuildManager.IsMasterPC() || !useMasterPC);
      }}

      //Override this in derived classes
      protected abstract void Sync();
   }
}
