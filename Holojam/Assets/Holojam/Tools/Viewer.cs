//Viewer.cs
//Created by Aaron C Gaudette on 07.07.16
//Rewrite of TrackedHeadset.cs, completed on 02.07.16

using UnityEngine;
using Holojam.Network;

namespace Holojam.Tools{
   [ExecuteInEditMode]
   public class Viewer : MonoBehaviour{
      public enum TrackingType{LEGACY,OPTICAL,IMU};
      public TrackingType trackingType = TrackingType.IMU;

      //Get tracking data from actor (recommended coupling), or directly from the view?
      public Actor actor = null;
      [HideInInspector] public HolojamView view = null;
      public Motive.Tag trackingTag = Motive.Tag.HEADSET1;
      public bool localSpace = false;

      const float correctionThreshold = 0.98f; //Lower values allow greater deviation without correction
      Quaternion correction = Quaternion.identity;

      const float differenceThreshold = 0.9995f; //Lower values allow correction at greater angular speeds
      float difference = 1;

      const float timestep = 0.01f;
      float lastTime = 0;
      Quaternion lastRotation = Quaternion.identity;

      //Update late to catch local space updates
      void LateUpdate(){
         //Flush extra components if necessary
         HolojamView[] views = GetComponents<HolojamView>();
         if((view==null && views.Length>0) || (view!=null && (views.Length>1 || views.Length==0))){
            foreach(HolojamView hv in views)DestroyImmediate(hv);
            view=null; //In case the view has been set to a prefab value
         }

         //Automatically add a HolojamView component if not using a reference actor
         if(actor==view)view=gameObject.AddComponent<HolojamView>() as HolojamView;
         else if(actor!=null && view!=null)DestroyImmediate(view);

         if(view!=null)view.Label=Motive.GetName(trackingTag);
         if(!Application.isPlaying)return;

         Vector3 sourcePosition = GetPosition();
         Quaternion sourceRotation = GetRotation();
         bool sourceTracked = GetTracked();

         //Don't use Camera.main (reference to Oculus' instantiated camera at runtime)
         //in the editor or standalone, reference the child camera instead
         Vector3 cameraPosition = Utility.IsMasterPC()?
            GetComponentInChildren<Camera>().transform.position:Camera.main.transform.position;
         //Negate Oculus' automatic head offset (variable reliant on orientation) independent of recenters
         transform.position+=sourcePosition-cameraPosition;

         if(sourceTracked){
            Quaternion imu = UnityEngine.VR.InputTracking.GetLocalRotation(UnityEngine.VR.VRNode.CenterEye);
            Quaternion optical = sourceRotation*Quaternion.Inverse(imu);

            //Calculate rotation difference since last timestep
            if(Time.time>lastTime+timestep){
               difference=Quaternion.Dot(imu,lastRotation);
               lastRotation=imu; lastTime=Time.time;
            }

            //Ignore local space rotation in the IMU calculations
            Quaternion localRotation = transform.rotation;
            if(actor!=null && actor.localSpace && actor.transform.parent!=null)
               localRotation=Quaternion.Inverse(actor.transform.parent.rotation)*transform.rotation;
            else if(actor==null && localSpace && transform.parent!=null)
               localRotation=Quaternion.Inverse(transform.parent.rotation)*transform.rotation;

            //Recalculate IMU correction if stale (generally on init/recenter)
            if(Quaternion.Dot(localRotation*imu,sourceRotation)<=correctionThreshold
               && difference>=differenceThreshold) //But not if the headset is moving quickly
               correction=optical;

            //IMU orientation (applied automatically by Oculus) is assumed below as a precondition
            switch(trackingType){
               case TrackingType.IMU: //IMU, absolutely oriented by optical tracking intermittently
                  if(Utility.IsMasterPC())
                     goto case TrackingType.OPTICAL; //Don't use IMU tracking in the editor or standalone
                  transform.rotation=correction;
                  break;
               case TrackingType.OPTICAL: //Purely optical tracking, no IMU
                  transform.rotation=optical;
                  break;
               case TrackingType.LEGACY:
                  transform.rotation=Quaternion.Slerp(transform.rotation,optical,Time.deltaTime);
                  break;
            }
         } else transform.rotation=correction; //Transition seamlessly to IMU when untracked

         //Apply local rotation if necessary
         if(actor!=null && actor.localSpace && actor.transform.parent!=null)
            transform.rotation=actor.transform.parent.rotation*transform.rotation;
         else if(actor==null && localSpace && transform.parent!=null)
            transform.rotation=transform.parent.rotation*transform.rotation;

         //Prints tracking status to VR debugger
         VRDebug.Println(actor!=null? actor.trackingTag.ToString():trackingTag.ToString());
      }
      //Get tracking data from desired source
      Vector3 GetPosition(){
         return actor!=null? actor.eyes:
         localSpace && transform.parent!=null?
            transform.parent.TransformPoint(view.RawPosition) : view.RawPosition;
      }
      Quaternion GetRotation(){return actor!=null?actor.rawOrientation:view.RawRotation;}
      bool GetTracked(){return actor!=null?actor.view.IsTracked:view.IsTracked;}
   }
}
