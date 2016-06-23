//Holobounds.cs
//Created by Aaron C Gaudette on 22.06.16

using UnityEngine;

namespace Holojam{
	[ExecuteInEditMode]
	public class Holobounds : MonoBehaviour{
		public TrackedObject calibrator;
		public Vector2[] bounds = new Vector2[4];
		public float floor;
		
		const int ceiling = 4;
		
		public void Calibrate(int i){
			if(calibrator==null){
				Debug.LogWarning("Holobounds: Calibrator not set");
				return;
			}
			Vector3 position = calibrator.transform.position;
			if(i<4)bounds[i]=new Vector2(position.x,position.z);
			else floor=position.y;
		}
		void OnDrawGizmos(){
			Gizmos.color=new Color(1,0.5f,0); //Orange
			for(int i=0;i<4;++i){
				Gizmos.DrawLine(Vertex(bounds[i]),Vertex(bounds[(i+1)%4]));
				Gizmos.DrawLine(Vertex(bounds[i]),Vertex(bounds[i])+Vector3.up*ceiling);
			}
		}
		Vector3 Vertex(Vector2 v){return new Vector3(v.x,floor,v.y);}
	}
}