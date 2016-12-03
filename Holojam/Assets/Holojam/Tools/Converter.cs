//Converter.cs
//Created by Aaron C Gaudette on 11.11.16
//

using UnityEngine;

namespace Holojam.Tools{
   public class Converter : MonoBehaviour{
      public BuildManager buildManager;
      public Scope extraData;
      public bool placeInEditor = false;
      public bool useTestIMU = false;

      [HideInInspector] public Network.View input, output;
      Transform imu;
      Network.View test;
      Quaternion raw, fit, correction = Quaternion.identity;

      void Awake(){
         if(buildManager==null){
            Debug.LogWarning("Converter: Build Manager reference is null!");
            return;
         }
         if(extraData==null){
            Debug.LogWarning("Converter: Extra data reference is null!");
            return;
         }
         if(BuildManager.IsMasterPC() && !placeInEditor && !useTestIMU)
            return;

         imu = buildManager.viewer.transform.GetChild(0);
         if(useTestIMU){
            test = gameObject.AddComponent<Network.View>() as Network.View;
            test.label = "IMU";
            test.scope = Network.Client.SEND_SCOPE;
            test.sending = false;
         }

         input = gameObject.AddComponent<Network.View>() as Network.View;
         input.label = Network.Canon.IndexToLabel(BuildManager.BUILD_INDEX,true);
         input.scope = "Holoscope"; //
         input.sending = false;

         output = gameObject.AddComponent<Network.View>() as Network.View;
         output.label = Network.Canon.IndexToLabel(BuildManager.BUILD_INDEX);
         output.scope = Network.Client.SEND_SCOPE;
         output.sending = true;
      }

      void Update(){
         //Editor debugging
         if(BuildManager.IsMasterPC()){
            if(placeInEditor){
               output.rawPosition = extraData.Localize(input.rawPosition);
               return;
            }else if(!useTestIMU)return;
         }

         //Update views
         input.label = Network.Canon.IndexToLabel(BuildManager.BUILD_INDEX,true);
         output.label = Network.Canon.IndexToLabel(BuildManager.BUILD_INDEX);

         //Get IMU data
         raw = useTestIMU?test.rawRotation:imu.localRotation;

         if(!input.tracked){
            //Rely on old data in untracked case
            output.rawRotation = extraData.Localize(raw*correction);
            output.rawPosition = extraData.Localize(
               input.rawPosition - output.rawRotation*Vector3.up*extraData.stem
            );
            return;
         }

         Vector3 left = new Vector3(
            input.rawRotation.x,input.rawRotation.y,input.rawRotation.z
         );

         Vector3 imuRight = raw*Vector3.right;
         Vector3 imuUp = raw*Vector3.up;
         Vector3 imuForward = raw*Vector3.forward;

         //degenerate case, note

         Quaternion lq = Quaternion.LookRotation(-left,Vector3.up);
         Quaternion iq = Quaternion.LookRotation(imuRight,Vector3.up);

         Quaternion difference = lq*Quaternion.Inverse(iq);
         Vector3 newForward = difference*imuForward;
         Vector3 newUp = difference*imuUp;

         Quaternion fit = Quaternion.LookRotation(newForward,newUp);
         correction = fit*Quaternion.Inverse(raw);

         output.rawRotation = extraData.Localize(fit);
         output.rawPosition = extraData.Localize(
            input.rawPosition - output.rawRotation*Vector3.up*extraData.stem
         );
      }
   }
}
