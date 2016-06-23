//BoundsOffset.cs
//Created by Aaron C Gaudette on 23.06.16

using UnityEngine;

namespace Holojam{
	[ExecuteInEditMode]
	public class BoundsOffset : MonoBehaviour{
		public Holobounds holobounds;
		public Vector3 offset = Vector3.zero;
		
		void Update(){
			if(holobounds!=null)transform.position=offset+holobounds.center;
		}
	}
}