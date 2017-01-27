//Remote.cs
//Created by Aaron C Gaudette on 03.01.17
//

using UnityEngine;
using Holojam.Tools;

public class Remote : Controller{
   public Converter.Device device = Converter.DEVICE_DEFAULT;

   protected override ProcessDelegate Process{get{return UpdateRemote;}}
   protected override string labelField{get{return "Remote";}}
   protected override string scopeField{get{return Holojam.Network.Client.SEND_SCOPE;}}
   protected override bool isSending{get{return !BuildManager.IsMasterPC();}}

   protected override int quadsCount{get{return 1;}}
   //Proxy
   Quaternion imu{set{UpdateQuad(0,value);}}

   protected void UpdateRemote(){
      if(!isSending)return;
      switch(device){
         case Converter.Device.CARDBOARD:
            imu = Camera.main.transform.rotation;
            break;
         case Converter.Device.DAYDREAM:
            imu = UnityEngine.VR.InputTracking.GetLocalRotation(
               UnityEngine.VR.VRNode.CenterEye
            );
            break;
      }
   }
}
