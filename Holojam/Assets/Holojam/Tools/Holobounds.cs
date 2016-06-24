//Holobounds.cs
//Created by Aaron C Gaudette on 22.06.16
//Access point and manager for playspace

using UnityEngine;

namespace Holojam{
	[ExecuteInEditMode]
	public class Holobounds : MonoBehaviour{
		public TrackedObject calibrator; //Tool for setting values
		public Vector2[] bounds = new Vector2[4]; //Corners
		public float floor = 0; //Floor Y
		public float ceiling = 3; //Ceiling Y -- only used for visualization
		
		//Reference values
		
		public Vector3 center{get{return Vertex(0.25f*(bounds[0]+bounds[1]+bounds[2]+bounds[3]));}}
		public Vector3 v0{get{return Vertex(bounds[0]);}}
		public Vector3 v1{get{return Vertex(bounds[1]);}}
		public Vector3 v2{get{return Vertex(bounds[2]);}}
		public Vector3 v3{get{return Vertex(bounds[3]);}}
		
		public Vector2 left{get{return 0.5f*(bounds[0]+bounds[3]);}}
		public Vector2 right{get{return 0.5f*(bounds[1]+bounds[2]);}}
		public Vector2 front{get{return 0.5f*(bounds[0]+bounds[1]);}}
		public Vector2 back{get{return 0.5f*(bounds[2]+bounds[3]);}}
		public float xRatio{get{
			return Vector2.Distance(left,right) / Vector2.Distance(front,back);
		}}
		public float yRatio{get{
			return Vector2.Distance(front,back) / Vector2.Distance(left,right);
		}}
		public float area{get{
			float a = 0;
			for(int i=0;i<4;++i)
				a+=bounds[(i+1)%4].x-bounds[i].x*
				bounds[(i+1)%4].y+bounds[i].y*0.5f;
			return Mathf.Abs(a);
		}}
		
		//Get distance to edge
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
		
		//Calibrate a specific corner (or floor value)
		public void Calibrate(int i){
			if(calibrator==null){
				Debug.LogWarning("Holobounds: Calibrator not set");
				return;
			}
			Vector3 position = calibrator.transform.position;
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
		Vector3 Vertex(Vector2 v){return new Vector3(v.x,floor,v.y);}
	}
}