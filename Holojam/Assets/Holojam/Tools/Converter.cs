//Converter.cs
//Created by Aaron C Gaudette on 11.11.16
//

using UnityEngine;

namespace Holojam.Tools{
   public class Converter : MonoBehaviour{
      public BuildManager buildManager;
      public bool debug = false;

      [HideInInspector] public Network.View input, output;
      Transform imu;
      Quaternion raw, fit, correction;

      const float stem = .125f; //TMP

      void Awake(){
         if(buildManager==null){
            Debug.LogWarning("Converter: Build Manager reference is null!");
            return;
         }
         imu = buildManager.viewer.transform.GetChild(0);

         input = gameObject.AddComponent<Network.View>() as Network.View;
         input.label = Network.Canon.IndexToLabel(BuildManager.BUILD_INDEX,true);
         input.scope = "Holoscope"; //
         input.sending = false;

         output = gameObject.AddComponent<Network.View>() as Network.View;
         output.label = Network.Canon.IndexToLabel(BuildManager.BUILD_INDEX);
         output.scope = debug && BuildManager.IsMasterPC()?"Editor":Network.Client.SEND_SCOPE;
         output.sending = true;
      }

      void Update(){
         //Editor debugging
         if(debug && BuildManager.IsMasterPC())
            output.rawPosition = input.rawPosition;
         else if(!debug)
            return;

         //Get IMU data
         raw = imu.rotation;

         if(!input.tracked){
            //Rely on old data in untracked case
            output.rawRotation = raw*correction;
            output.rawPosition = input.rawPosition - output.rawRotation*Vector3.up*stem;
            return;
         }

         Vector3 left = new Vector3(
            input.rawRotation.x,input.rawRotation.y,input.rawRotation.z
         );

         Vector3 imuRight = raw*Vector3.right;
         Vector3 imuUp = raw*Vector3.up;
         Vector3 imuForward = raw*Vector3.forward;
         //Find the closest vector to real up
         float dotx = Mathf.Abs(Vector3.Dot(imuRight,Vector3.up));
         float doty = Mathf.Abs(Vector3.Dot(imuUp,Vector3.up));
         float dotz = Mathf.Abs(Vector3.Dot(imuForward,Vector3.up));
         float max = Mathf.Max(dotx,doty,dotz);

         //The dotz case is degenerate, nothing to do but rely on old data.
         Quaternion fit = raw;

         //Stable cases
         if(max==doty){
            Vector3 forward = Vector3.Cross(left,imuUp);
            fit = Quaternion.LookRotation(forward,imuUp);
         }
         else if(max==dotz){
            Vector3 up = Vector3.Cross(left,imuForward);
            fit = Quaternion.LookRotation(imuForward,up);
         }
         correction = fit*Quaternion.Inverse(raw); //Get difference

         output.rawRotation = raw*correction;
         output.rawPosition = input.rawPosition - output.rawRotation*Vector3.up*stem;
      }
   }
}
