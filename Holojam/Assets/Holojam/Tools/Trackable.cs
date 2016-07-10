//Trackable.cs
//Created by Aaron C Gaudette on 09.07.16
//Base class for trackable entities

using UnityEngine;
using Holojam.Network;

namespace Holojam{
	[ExecuteInEditMode, RequireComponent(typeof(HolojamView))]
	public class Trackable : MonoBehaviour{
		public Motive.Tag trackingTag = Motive.Tag.BOX1;
		
		HolojamView holojamView = null;
		public HolojamView view{get{
			if(holojamView==null)holojamView=GetComponent<HolojamView>();
			return holojamView;
		}}
		
		void Awake(){view.Label=Motive.GetName(trackingTag);} //Cache label (in edit mode)
		
		//Override these in derived classes
		protected virtual void Update(){
			UpdateTracking();
		}
		protected virtual void UpdateTracking(){
			if(!Application.isPlaying)return; //Safety check
			
			//By default, assigns position and rotation injectively
			if(view.IsTracked){
				transform.position=view.RawPosition;
				transform.rotation=view.RawRotation;
			}
		}
		
		void OnDrawGizmosSelected(){
			Gizmos.color=Color.gray;
			//Pivot
			Drawer.Circle(transform.position,Vector3.up,Vector3.forward,0.18f);
			Gizmos.DrawRay(transform.position,transform.forward*0.18f);
			Gizmos.DrawLine(transform.position-0.03f*Vector3.left,transform.position+0.03f*Vector3.left);
			Gizmos.DrawLine(transform.position-0.03f*Vector3.forward,transform.position+0.03f*Vector3.forward);
		}
	}
}