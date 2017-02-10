//Remote.cs
//Created by Aaron C Gaudette on 03.01.17
//

using UnityEngine;
using Holojam.Tools;

public class Remote : Controller{
   public BuildManager.Device device = BuildManager.DEVICE_DEFAULT;

   protected override ProcessDelegate Process{get{return UpdateRemote;}}
   public override string labelField{get{return "Remote";}}
   public override string scopeField{get{return Holojam.Network.Client.SEND_SCOPE;}}
   public override bool isSending{get{return !BuildManager.IsMasterClient();}}

   public override int quadCount{get{return 1;}}
   //Proxy
   Quaternion imu{set{SetQuad(0,value);}}

   void UpdateRemote(){
      if(!isSending)return;
      switch(device){
         case BuildManager.Device.CARDBOARD:
            imu = Camera.main.transform.rotation;
            break;
         case BuildManager.Device.DAYDREAM:
            imu = UnityEngine.VR.InputTracking.GetLocalRotation(
               UnityEngine.VR.VRNode.CenterEye
            );
            break;
      }
   }
}
