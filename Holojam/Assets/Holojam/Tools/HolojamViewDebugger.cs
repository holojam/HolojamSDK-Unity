//HolojamViewDebugger.cs
//Created by Aaron C Gaudette on 13.07.16

using UnityEngine;
using Holojam.Network;

namespace Holojam.Tools{
	[ExecuteInEditMode, RequireComponent(typeof(HolojamView))]
	public class HolojamViewDebugger : MonoBehaviour{
		public string label;
		
		public Vector3 rawPosition;
		public Quaternion rawRotation;
		public int bits;
		public string blob;
		
		public HolojamView view{get{
			if(holojamView==null)holojamView=GetComponent<HolojamView>();
			return holojamView;
		}}
		HolojamView holojamView = null;
		
		void Update(){
			view.Label=label;
			view.IsMine=false;
			
			rawPosition=view.RawPosition;
			rawRotation=view.RawRotation;
			bits=view.Bits;
			blob=view.Blob;
		}
	}
}