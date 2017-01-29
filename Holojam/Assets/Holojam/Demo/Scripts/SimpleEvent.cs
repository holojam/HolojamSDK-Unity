//SimpleEvent.cs
//Created by Aaron C Gaudette on 23.01.17

using UnityEngine;

public abstract class SimpleEvent : Holojam.Tools.Controller{
   public string label = "Event";
   public string scope = "";

   protected override sealed ProcessDelegate Process{get{return UpdateEvent;}}

   public override string labelField{get{return label;}}
   public override string scopeField{get{return scope;}}
   public override sealed bool isSending{get{return true;}}
   public override sealed bool isIgnoringTracking{get{return true;}}

   //Helper function
   public void Push(){Holojam.Network.Client.PushEvent(view);}

   protected abstract void UpdateEvent();
}
