// SimpleEvent.cs
// Created by Holojam Inc. on 23.01.17

using UnityEngine;

public abstract class SimpleEvent : Holojam.Tools.Controller {
  public string label = "Event";
  public string scope = "";

  protected override sealed ProcessDelegate Process { get { return UpdateEvent; } }

   public override string Label { get { return label; } }
   public override string Scope { get { return scope; } }
   public override sealed bool Sending { get { return true; } }
   public override sealed bool IgnoringTracking { get { return true; } }

   // Helper function
   public void Push() {
      Holojam.Network.Client.PushEvent(view);
   }

  protected abstract void UpdateEvent();
}
