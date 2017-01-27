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

      protected override string labelField{get{return label;}}
      //Empty scope defaults to client send scope
      protected override string scopeField{get{return scope==""?Network.Client.SEND_SCOPE:scope;}}
      protected override bool isSending{get{
         return sending && (BuildManager.IsMasterPC() || !useMasterPC);
      }}

      //Override this in derived classes
      protected abstract void Sync();
   }
}
