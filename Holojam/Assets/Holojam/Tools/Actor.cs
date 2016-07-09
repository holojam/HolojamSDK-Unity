//Actor.cs
//Created by Aaron C Gaudette on 23.06.16
//Umbrella class for accessing player (headset user) data in a generic manner

using UnityEngine;
using Holojam.Network;

namespace Holojam{
	[RequireComponent(typeof(HolojamView))]
	public class Actor : MonoBehaviour {
		public string name = "Actor";
		public Color motif = Color.white; //Useful color identifier, optional for rendering
		public GameObject mask; //This object is disabled for build actors by the manager
		
		HolojamView holojamView = null;
		public HolojamView view{get{
			if(holojamView==null)holojamView=GetComponent<HolojamView>();
			return holojamView;
		}}
		public int index{get{ //Temporary fix until new labeling system is implemented
			switch(view.label){
				case "VR1": return 0;
				case "VR2": return 1;
				case "VR3": return 2;
				case "VR4": return 3;
			}
			return -1;
		}}
		public bool managed{get{
			return transform.parent!=null && transform.parent.GetComponent<ActorManager>()!=null;
		}}
		
		//Override these in derived classes for custom unique implementation
		protected virtual void Update(){ //Update tracking data (position, rotation) and manage the untracked state here
			if(view.IsTracked){
				transform.position=view.RawPosition;
				transform.rotation=view.RawRotation;
			}
		}
		public virtual void ApplyMotif(){} //Do something with the motif (color)
		//These accessors should always reference assigned data (e.g. transform.position), not source (raw) data
		public virtual Vector3 eyes{
			get{return transform.position;}
		}
		public virtual Quaternion orientation{
			//Be careful not to map rotation to anything other than the user's actual head movement
			//unless you absolutely know what you're doing. The Viewer (headset) uses a custom
			//tracking algorithm and relies on this accessor to provide absolute truth.
			get{return transform.rotation;}
		}
		
		//Useful derived accessors
		public Vector3 look{get{return orientation*Vector3.forward;}}
		public Vector3 up{get{return orientation*Vector3.up;}}
		public Vector3 left{get{return orientation*Vector3.left;}}
		
		//Useful (goggles) visualization for edge of GearVR headset
		void OnDrawGizmos(){
			Gizmos.color=motif;
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