//Viewer.cs
//Created by Aaron C Gaudette on 07.07.16
//Rewrite of TrackedHeadset.cs, completed on 02.07.16

using UnityEngine;
using Holojam.Network;

namespace Holojam{
	[ExecuteInEditMode]
	public class Viewer : MonoBehaviour{
		public enum TrackingType{LEGACY,OPTICAL,IMU};
		public TrackingType trackingType = TrackingType.IMU;
		
		//Get tracking data from actor (recommended coupling), or directly from the view?
		public Actor actor = null;
		[HideInInspector] public HolojamView view = null;
		public Motive.Tag trackingTag = Motive.Tag.HEADSET1;
		
		const float correctionThreshold = 0.98f; //Lower values allow greater deviation without correction
		Quaternion correction = Quaternion.identity;
		
		const float differenceThreshold = 0.9995f; //Lower values allow correction at greater angular speeds
		float difference = 1;
		
		const float timestep = 0.01f;
		float lastTime = 0;
		Quaternion lastRotation = Quaternion.identity;
		
		void Update(){
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
			
			//Negate Oculus' automatic head offset (variable reliant on orientation) independent of recenters
			transform.position+=sourcePosition-Camera.main.transform.position;
			
			if(sourceTracked){
				Quaternion imu = UnityEngine.VR.InputTracking.GetLocalRotation(UnityEngine.VR.VRNode.CenterEye);
				Quaternion optical = sourceRotation*Quaternion.Inverse(imu);
				
				//Calculate rotation difference since last timestep
				if(Time.time>lastTime+timestep){
					difference=Quaternion.Dot(imu,lastRotation);
					lastRotation=imu; lastTime=Time.time;
				}
				
				//Recalculate IMU correction if stale (generally on init/recenter)
				if(Quaternion.Dot(transform.rotation*imu,sourceRotation)<=correctionThreshold
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
		//Get tracking data from desired source
		Vector3 GetPosition(){return actor!=null?actor.eyes:view.RawPosition;}
		Quaternion GetRotation(){return actor!=null?actor.rawOrientation:view.RawRotation;}
		bool GetTracked(){return actor!=null?actor.view.IsTracked:view.IsTracked;}
	}
}