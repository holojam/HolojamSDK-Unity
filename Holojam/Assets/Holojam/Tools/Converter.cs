//Converter.cs
//Created by Aaron C Gaudette on 11.11.16
//

#define SMOOTH

using UnityEngine;

namespace Holojam.Tools{
   public class Converter : MonoBehaviour{
      public BuildManager buildManager;
      public Scope extraData;
      public bool placeInEditor = false;
      public bool useTestIMU = false;

      const float POSITION_DAMPING = 5;
      const float ROTATION_DAMPING = 60;

      Vector3 lastPosition = Vector3.zero;

      [HideInInspector] public Network.View input, output;
      Transform imu;
      Network.View test;
      Quaternion raw, correction, correctionTarget = Quaternion.identity;

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
         input.scope = "Holoscope";
         input.sending = false;

         output = gameObject.AddComponent<Network.View>() as Network.View;
         output.label = Network.Canon.IndexToLabel(BuildManager.BUILD_INDEX);
         output.scope = Network.Client.SEND_SCOPE;
         output.sending = true;
      }

      void Update(){
         #if(SMOOTH)
            Vector3 inputPosition = Smooth(input.rawPosition,ref lastPosition);
         #else
            Vector3 inputPosition = input.rawPosition;
         #endif

         //Editor debugging
         if(BuildManager.IsMasterPC()){
            if(placeInEditor){
               output.rawPosition = inputPosition;
               return;
            }else if(!useTestIMU)return;
         }

         //Update views
         input.label = Network.Canon.IndexToLabel(BuildManager.BUILD_INDEX,true);
         output.label = Network.Canon.IndexToLabel(BuildManager.BUILD_INDEX);

         //Get IMU data
         raw = useTestIMU?test.rawRotation:imu.localRotation;

         //Update target if tracked
         if(input.tracked){
            //Read in secondary vector
            Vector3 left = new Vector3(
               input.rawRotation.x,input.rawRotation.y,input.rawRotation.z
            ).normalized;

            Vector3 imuUp = raw*Vector3.up;
            Vector3 imuForward = raw*Vector3.forward;
            Vector3 imuRight = raw*Vector3.right;

            //Compare orientations relative to gravity
            Quaternion difference = Quaternion.LookRotation(-left,Vector3.up)
               * Quaternion.Inverse(Quaternion.LookRotation(imuRight,Vector3.up));

            Vector3 newForward = difference*imuForward;
            Vector3 newUp = difference*imuUp;

            //Ideal rotation
            Quaternion target = Quaternion.LookRotation(newForward,newUp);
            correctionTarget = target*Quaternion.Inverse(raw);
         }

         //Smoothly interpolate correction
         float delta = 0.5f*Quaternion.Dot(correction,correctionTarget)+0.5f;
         delta*=delta;

         #if(SMOOTH)
            correction = Quaternion.Slerp(
               correction,correctionTarget,delta*ROTATION_DAMPING
            );
         #else
            correction = correctionTarget;
         #endif

         //Update output
         output.rawRotation = extraData.Localize(correction*raw);
         output.rawPosition = extraData.Localize(
            inputPosition - output.rawRotation*Vector3.up*extraData.stem
         );
      }

      //Smooth signal while minimizing perceived latency
      Vector3 Smooth(Vector3 target, ref Vector3 last){
         target = Vector3.Lerp(
            target,last,(last-target).sqrMagnitude*POSITION_DAMPING
         );
         last = target;
         return target;
      }
   }
}
