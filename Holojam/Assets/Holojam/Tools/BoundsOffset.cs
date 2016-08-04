//BoundsOffset.cs
//Created by Aaron C Gaudette on 23.06.16
//Automatically centers object to holobounds and offsets

using UnityEngine;

namespace Holojam{
	[ExecuteInEditMode]
	public class BoundsOffset : MonoBehaviour{
		public Holobounds holobounds;
		public Vector3 offset = Vector3.zero;
		
		void LateUpdate(){
			if(holobounds!=null){
				transform.position=holobounds.Offset(offset);
				transform.rotation=holobounds.rotation;
			}
		}
	}
}