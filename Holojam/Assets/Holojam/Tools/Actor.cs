//Actor.cs
//Created by Aaron C Gaudette on 23.06.16
//Umbrella class for accessing player (headset user) data in a generic manner

using UnityEngine;
using Holojam.Server;

namespace Holojam{
	public class Actor : TrackedObject{
		public string name = "Actor";
		public Color motif = Color.white; //Useful color identifier, optional for rendering
		public GameObject mask; //This object is disabled for build actors by the manager
		
		//Override these in derived classes for specific implementation
		public virtual Vector3 eyes{
			get{return trackedPosition;}
		}
		public virtual Quaternion orientation{
			get{return trackedRotation;}
		}
		
		public bool tracking{get{return isTracked;}}
		public bool managed{get{
			return transform.parent!=null && transform.parent.GetComponent<ActorManager>()!=null;
		}}
		
		public int index{get{return (int)liveObjectTag;}}
		public Vector3 look{get{return orientation*Vector3.forward;}}
		public Vector3 up{get{return orientation*Vector3.up;}}
		public Vector3 left{get{return orientation*Vector3.left;}}
		
		protected override void Update(){
			UpdateTracking();
			ApplyTracking();
		}
		
		//Override these in derived classes for specific implementation
		public virtual void ApplyTracking(){ //Assign tracking data
			if(tracking){
				transform.position=eyes;
				transform.rotation=orientation;
			}
		}
		public virtual void ApplyMotif(){} //Do something with the motif (color)
		
		void OnDrawGizmos(){
			Gizmos.color=motif;
			//Useful visualization for edge of GearVR headset
			Vector3 offset = eyes+look*0.015f;
			DrawCircle(offset+left*0.035f,look,up,0.03f);
			DrawCircle(offset-left*0.035f,look,up,0.03f);
			//Reference forward vector
			Gizmos.DrawRay(offset,look);
		}
		
		private const int circleResFactor = 128; //Quality factor for drawing circles
		//Gizmo circle-drawing tool
		void DrawCircle(Vector3 position, Vector3 direction, Vector3 up, float radius = 0.1f){
			Vector3.Normalize(direction); Vector3.Normalize(up);
			int res = (int)(circleResFactor*Mathf.Sqrt(radius)); //Approximate resolution based on radius
			
			float theta = 2*Mathf.PI/res;
			Vector3 cache = Vector3.zero;
			for(int i=0;i<=res;++i){
				Vector3 point=
					up*radius*Mathf.Sin(theta*i)+
					Vector3.Cross(direction,up)*radius*Mathf.Cos(theta*i)+ 
					position;
				if(i>0)Gizmos.DrawLine(cache,point);
				cache=point;
			}
		}
	}
}