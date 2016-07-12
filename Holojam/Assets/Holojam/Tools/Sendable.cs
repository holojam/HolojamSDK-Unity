//Sendable.cs
//Created by Aaron C Gaudette on 11.07.16

using UnityEngine;
using Holojam.Network;

namespace Holojam{
	[ExecuteInEditMode, RequireComponent(typeof(HolojamView))]
	public class Sendable : MonoBehaviour{ //syncable?
		public string label = "Label";
		public bool useMasterPC = true;
		public bool sending = true; //managed
		public bool linkTransform = false; //dropdown
		
		//Manage view
		public HolojamView view{get{
			if(holojamView==null)holojamView=GetComponent<HolojamView>();
			return holojamView;
		}}
		HolojamView holojamView = null;
		
		void UpdateView(){
			view.Label=label;
			view.IsMine=sending;
			
			if(linkTransform){
				if(sending){
					view.RawPosition=transform.position;
					view.RawRotation=transform.rotation;
				}
				else{
					transform.position=view.RawPosition;
					transform.rotation=view.RawRotation;
				}
			}
		}
		
		//custom scripts have to have a case check for writing/reading
		//rethink workflow
		
		void Update(){
			sending=sending || (useMasterPC && IsMasterPC());
			UpdateView();
			//if(Application.isPlaying)return;
		}
		//
		bool IsMasterPC(){
			switch(Application.platform){
				case RuntimePlatform.OSXEditor: return true;
				case RuntimePlatform.OSXPlayer: return true;
				case RuntimePlatform.WindowsEditor: return true;
				case RuntimePlatform.WindowsPlayer: return true;
				case RuntimePlatform.LinuxPlayer: return true;
			}
			return false;
		}
	}
}