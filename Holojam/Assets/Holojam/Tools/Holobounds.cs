//Holobounds.cs
//Created by Aaron C Gaudette on 22.06.16
//Playspace manager and access point

using UnityEngine;
using Holojam.Network;

namespace Holojam{
	[ExecuteInEditMode]
	public class Holobounds : MonoBehaviour{
		public HolojamView calibrator; //Tool for setting values
		public Vector2[] bounds = new Vector2[4]; //Corners (FL,FR,BR,BL)
		public float floor = 0; //Floor Y
		public float ceiling = 3; //Ceiling Y -- not used for tracking
		
		//Additional reference values & functions
		
		public Vector3 center{get{return Vertex(0.25f*(bounds[0]+bounds[1]+bounds[2]+bounds[3]));}}
		public Vector3 Corner(int i){return Vertex(bounds[i]);}
		public Vector3 Upper(int i){return Corner(i)+Vector3.up*ceiling;}
		public Vector2 Side(int i){return 0.5f*(bounds[i++]+bounds[i%4]);} //Front, right, back, left
		
		public float xRatio{get{
			return Vector2.Distance(Side(3),Side(1)) / Vector2.Distance(Side(0),Side(2));
		}}
		public float area{get{
			float a = 0;
			for(int i=0;i<4;++i)
				a+=bounds[(i+1)%4].x-bounds[i].x*
				bounds[(i+1)%4].y+bounds[i].y*0.5f;
			return Mathf.Abs(a);
		}}
		
		//Get distance from point to edge of boundary
		public float Distance(Vector3 target){
			Vector2 t = new Vector2(target.x,target.z);
			
			float minDistance = -1;
			for(int i=0;i<4;++i){
				float d=minDistance+1;
				
				float l2 = Vector2.Distance(bounds[i],bounds[(i+1)%4]); l2*=l2;
				if(l2==0)d=Vector2.Distance(bounds[i],t);
				float p = Mathf.Max(0,Mathf.Min(1,Vector2.Dot(t-bounds[i],bounds[(i+1)%4]-bounds[i])/l2));
				Vector2 proj = bounds[i]+p*(bounds[(i+1)%4]-bounds[i]);
				d=Vector2.Distance(t,proj);
				
				if(d<minDistance || minDistance==-1)minDistance=d;
			}
			return minDistance;
		}
		
		Vector3 Vertex(Vector2 v){return new Vector3(v.x,floor,v.y);}
		
		//Calibrate a specific corner (or floor value)
		public void Calibrate(int i){
			if(calibrator==null){
				Debug.LogWarning("Holobounds: Calibrator not set");
				return;
			}
			Vector3 position = calibrator.RawPosition;
			if(i<4)bounds[i]=new Vector2(position.x,position.z);
			else if(i==4)floor=position.y;
			else ceiling=position.y;
		}
		//Draw for editor & debug
		void OnDrawGizmos(){
			Gizmos.color=new Color(1,0.5f,0); //Orange
			for(int i=0;i<4;++i){
				//Edges and corners
				Gizmos.DrawLine(Vertex(bounds[i]),Vertex(bounds[(i+1)%4]));
				Gizmos.DrawRay(Vertex(bounds[i]),Vector3.up*ceiling);
			}
		}
		
		//Save and load calibration data
		static bool hasPlayed = false;
		void Start(){
			if(!Application.isPlaying && hasPlayed){
				for(int i=0;i<4;++i){
					bounds[i].x=PlayerPrefs.GetFloat("Holobounds_Corner"+i+"_x");
					bounds[i].y=PlayerPrefs.GetFloat("Holobounds_Corner"+i+"_y");
				}
			} else if(Application.isPlaying)hasPlayed=true;
		}
		void OnApplicationQuit(){Save();}
		void Save(){
			for(int i=0;i<4;++i){
				PlayerPrefs.SetFloat("Holobounds_Corner"+i+"_x",bounds[i].x);
				PlayerPrefs.SetFloat("Holobounds_Corner"+i+"_y",bounds[i].y);
			}
		}
	}
}