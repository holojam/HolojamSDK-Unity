//Actor.cs
//Created by Aaron C Gaudette on 23.06.16

using UnityEngine;

namespace Holojam{
	[ExecuteInEditMode]
	[RequireComponent(typeof(TrackedObject))]
	public class Actor : MonoBehaviour{
		public Color color = Color.red;
		public int index{get{return (int)(GetComponent<TrackedObject>().liveObjectTag);}}
		
		//If shell (non-build actor), assign colors with Motif tag
		public void ApplyMotif(){
			if(Application.isPlaying){
				Transform shell = transform.Find("Shell");
				if(shell!=null)
					foreach(Renderer r in shell.GetComponentsInChildren<Renderer>())
						if(r.gameObject.tag=="Motif")r.material.color=color;
			}
		}
		
		void OnDrawGizmos(){
			Gizmos.color=color;
			//Useful visualization for edge of GearVR headset
			Vector3 offset = transform.position+transform.forward*0.015f;
			DrawCircle(offset-transform.right*0.035f,transform.forward,transform.up,0.03f);
			DrawCircle(offset+transform.right*0.035f,transform.forward,transform.up,0.03f);
			//Reference forward vector
			Gizmos.DrawRay(offset,transform.forward);
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