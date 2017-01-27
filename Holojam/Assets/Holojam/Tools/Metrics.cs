//Metrics.cs
//Created by Aaron C Gaudette on 10.01.16

using UnityEngine;

namespace Holojam.Tools{
   public class Metrics : Controller{
      public bool disable = false;

      protected override ProcessDelegate Process{get{return UpdateMetrics;}}

      protected override string labelField{get{return "MetricsACK";}}
      protected override string scopeField{get{return "Holoscope";}}
      protected override bool isSending{get{return true;}}
      protected override bool isIgnoringTracking{get{return true;}} //Event

      protected override int floatsCount{get{return 1;}}

      void Start(){
         if(disable)return;
         Network.Notifier.AddSubscriber(Route,"Metrics");
      }

      void OnDisable(){
         if(disable)return;
         Network.Notifier.RemoveSubscriber(Route,"Metrics");
      }

      void Route(Network.View input){
         Network.Client.PushEvent(view);
      }

      //Send back the time to render
      void UpdateMetrics(){
         UpdateFloat(0,Time.smoothDeltaTime*1000);
      }
   }
}
