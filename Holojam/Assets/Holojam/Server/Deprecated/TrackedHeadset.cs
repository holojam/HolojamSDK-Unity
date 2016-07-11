#pragma warning disable 0618 //Deprecated

using UnityEngine;

namespace Holojam{
	public class TrackedHeadset : TrackedObject{
		public enum TrackingType{LEGACY,OPTICAL,IMU};
		public TrackingType trackingType = TrackingType.IMU;
		
		const float correctionThreshold = 0.98f; //Lower values allow greater deviation without correction
		Quaternion correction = Quaternion.identity;
		
		const float differenceThreshold = 0.9995f; //Lower values allow correction at greater angular speeds
		float difference = 1;
		
		const float timestep = 0.01f;
		float lastTime = 0;
		Quaternion lastRotation = Quaternion.identity;
		
		protected override void Update(){
			UpdateTracking();
			//Negate Oculus' automatic head offset (variable reliant on orientation) independent of recenters
			transform.position+=trackedPosition-Camera.main.transform.position;
			
			if(IsTracked){
				Quaternion imu = UnityEngine.VR.InputTracking.GetLocalRotation(UnityEngine.VR.VRNode.CenterEye);
				Quaternion optical = trackedRotation*Quaternion.Inverse(imu);
				
				//Calculate rotation difference since last timestep
				if(Time.time>lastTime+timestep){
					difference=Quaternion.Dot(imu,lastRotation);
					lastRotation=imu; lastTime=Time.time;
				}
				
				//Recalculate IMU correction if stale (generally on init/recenter)
				if(Quaternion.Dot(transform.rotation*imu,trackedRotation)<=correctionThreshold
					&& difference>=differenceThreshold) //But not if the headset is moving quickly
					correction=optical;
				
				//IMU orientation (applied automatically by Oculus) is assumed below as a precondition
				switch(trackingType){
					case TrackingType.IMU: //IMU, absolutely oriented by optical tracking intermittently
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
		}
	}
}