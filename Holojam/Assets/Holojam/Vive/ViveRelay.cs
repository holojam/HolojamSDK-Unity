//ViveRelay.cs
//Created by Aaron C Gaudette on 26.01.17

using UnityEngine;

namespace Holojam.Vive{
   public class ViveRelay : Tools.Controller{
      protected override ProcessDelegate Process{get{return Relay;}}

      public override string labelField{
         get{return Network.Canon.IndexToLabel(Tools.BuildManager.BUILD_INDEX);}
      }
      public override string scopeField{get{return "HolojamVive";}}
      public override bool isSending{get{return !Holojam.Tools.BuildManager.IsMasterPC();}}

      public override int tripleCount{get{return 1;}}
      public override int quadCount{get{return 1;}}

      void Relay(){
         SetTriple(0,transform.position);
         SetQuad(0,transform.rotation);
      }
   }
}
