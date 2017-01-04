//Remote.cs
//Created by Aaron C Gaudette on 03.01.17
//

using UnityEngine;
using Holojam.Tools;

public class Remote : Synchronizable{
   public Converter.Device device = Converter.DEVICE_DEFAULT;

   //TODO: Default variables

   protected override void Sync(){
      if(BuildManager.IsMasterPC() || !sending)return;
      switch(device){
         case Converter.Device.CARDBOARD:
            synchronizedQuaternion = Camera.main.transform.rotation;
            break;
         case Converter.Device.DAYDREAM:
            synchronizedQuaternion = UnityEngine.VR.InputTracking.GetLocalRotation(
               UnityEngine.VR.VRNode.CenterEye
            );
            break;
      }
   }
}
