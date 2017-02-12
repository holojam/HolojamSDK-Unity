// Metrics.cs
// Created by Holojam Inc. on 10.01.16

using UnityEngine;

/// <summary>
/// Switch to internal use.
/// </summary>
namespace Holojam.Tools{
   public class Metrics : Controller{
      public bool disable = false;

      protected override ProcessDelegate Process{get{return UpdateMetrics;}}

      public override string Label{get{return "MetricsACK";}}
      public override string Scope{get{return "Holoscope";}}
      public override bool Sending{get{return true;}}
      public override bool IgnoringTracking{get{return true;}} // Event

      public override int FloatCount{get{return 1;}}
      public override int IntCount{get{return 1;}}

      void Start(){
         if(disable)return;
         Network.Notifier.AddSubscriber(Route,"Metrics");
      }

      void OnDisable(){
         if(disable)return;
         Network.Notifier.RemoveSubscriber(Route,"Metrics");
      }

      void Route(Network.View input){
         SetInt(0,input.ints[0]); // Copy over tick
         Network.Client.PushEvent(view);
      }

      // Send back the time to render
      void UpdateMetrics(){
         SetFloat(0,Time.smoothDeltaTime*1000);
      }
   }
}
