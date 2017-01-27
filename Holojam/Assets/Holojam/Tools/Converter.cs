//Converter.cs
//Created by Aaron C Gaudette on 11.11.16
//

#define SMOOTH

using UnityEngine;

namespace Holojam.Tools{
   public class Converter : MonoBehaviour{
      public BuildManager buildManager;
      public Scope extraData;

      public enum Device{
         CARDBOARD,DAYDREAM,VIVE
      };
      public const Device DEVICE_DEFAULT = Device.DAYDREAM;
      public Device device = DEVICE_DEFAULT;

      public enum DebugMode{NONE,POSITION,REMOTE}
      public DebugMode debugMode = DebugMode.NONE;

      const float POSITION_DAMPING = 5;
      const float ROTATION_DAMPING = 0.01f; //0.001f;

      #if SMOOTH
      Vector3 lastPosition = Vector3.zero;
      #endif

      Network.View input, output;
      Transform imu;
      Network.View test;
      Quaternion raw, correction, correctionTarget = Quaternion.identity;

      //Proxies
      public Vector3 outputPosition{
         get{return output.triples[0];}
         set{output.triples[0] = value;}
      }
      public Quaternion outputRotation{
         get{return output.quads[0];}
         set{output.quads[0] = value;}
      }
      public bool hasInput{get{return input.tracked;}}

      void Awake(){
         if(device==Device.VIVE)
            return;

         //Ignore debug flags on phones
         if(!BuildManager.IsMasterPC())
            debugMode = DebugMode.NONE;

         if(buildManager==null){
            Debug.LogWarning("Converter: Build Manager reference is null!");
            return;
         }
         if(extraData==null){
            Debug.LogWarning("Converter: Extra data reference is null!");
            return;
         }
         if(BuildManager.IsMasterPC() && debugMode==DebugMode.NONE)
            return;

         imu = buildManager.viewer.transform.GetChild(0);
         if(debugMode==DebugMode.REMOTE){
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

         //Allocate
         input.triples = new Vector3[2];
         output.triples = new Vector3[1];
         output.quads = new Quaternion[1];
      }

      void Update(){
         if(device==Device.VIVE)
            return;

         //Editor debugging
         if(BuildManager.IsMasterPC()){
            if(debugMode==DebugMode.POSITION){
               #if(SMOOTH)
                  outputPosition = extraData.Localize(
                     Smooth(input.triples[0],ref lastPosition)
                  );
               #else
                  outputPosition = extraData.Localize(input.triples[0]);
               #endif
               return;
            }else if(debugMode==DebugMode.NONE)return;
         }

         #if(SMOOTH)
            Vector3 inputPosition = Smooth(input.triples[0],ref lastPosition);
         #else
            Vector3 inputPosition = input.triples[0];
         #endif

         //Update views
         input.label = Network.Canon.IndexToLabel(BuildManager.BUILD_INDEX,true);
         output.label = Network.Canon.IndexToLabel(BuildManager.BUILD_INDEX);

         //Get IMU data
         if(debugMode==DebugMode.REMOTE)
            raw = test.quads[0];
         else switch(device){
            case Device.CARDBOARD:
               raw = imu.localRotation;
               break;
            case Device.DAYDREAM:
               raw = UnityEngine.VR.InputTracking.GetLocalRotation(
                  UnityEngine.VR.VRNode.CenterEye
               );
               break;
         }

         //Update target if tracked
         if(input.tracked){
            //Read in secondary vector
            Vector3 left = new Vector3(
               input.triples[1].x,input.triples[1].y,input.triples[1].z
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
         outputRotation = extraData.Localize(correction*raw);
         outputPosition = extraData.Localize(
            inputPosition - outputRotation*Vector3.up*extraData.stem
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
