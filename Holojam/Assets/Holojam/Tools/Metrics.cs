// Metrics.cs
// Created by Holojam Inc. on 10.01.16

using UnityEngine;

namespace Holojam.Tools{

   /// <summary>
   /// Routes round-trip packet and tags with Unity render time.
   /// </summary>
   public sealed class Metrics : Controller {
      public bool disable = false;

      protected override ProcessDelegate Process { get { return UpdateMetrics; } }

      public override string Label { get { return "MetricsACK"; } }
      public override string Scope { get { return "Holoscope"; } }

      public override bool Sending { get { return true;}}
      public override bool Deaf { get { return true; } } // Event

      public override int FloatCount { get{ return 1; } }
      public override int IntCount { get { return 1; } }

      void Start() {
         if (disable) return;
         Network.Notifier.AddSubscriber(Route, "Metrics");
      }

      void OnDisable(){
         if (disable) return;
         Network.Notifier.RemoveSubscriber(Route, "Metrics");
      }

      void Route(Network.View input) {
         SetInt(0, input.ints[0]); // Copy over tick
         PushEvent();
      }

      // Send back the time to render
      void UpdateMetrics() {
         SetFloat(0, Time.smoothDeltaTime * 1000);
      }
   }
}
