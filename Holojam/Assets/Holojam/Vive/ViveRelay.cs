//ViveRelay.cs
//Created by Aaron C Gaudette on 26.01.17

using UnityEngine;

namespace Holojam.Vive{
   public class ViveRelay : Tools.Controller{
      protected override ProcessDelegate Process{get{return Relay;}}

      protected override string labelField{
         get{return Network.Canon.IndexToLabel(Tools.BuildManager.BUILD_INDEX);}
      }
      protected override string scopeField{get{return "HolojamVive";}}
      protected override bool isSending{get{return true;}}

      protected override int triplesCount{get{return 1;}}
      protected override int quadsCount{get{return 1;}}

      void Relay(){
         UpdateTriple(0,transform.position);
         UpdateQuad(0,transform.rotation);
      }
   }
}
