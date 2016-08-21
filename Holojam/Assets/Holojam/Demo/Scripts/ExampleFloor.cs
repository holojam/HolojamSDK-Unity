//ExampleRoom.cs
//Created by Aaron C Gaudette on 23.06.16

using UnityEngine;
[ExecuteInEditMode]

public class ExampleFloor : MonoBehaviour{
	public Holojam.Tools.Holobounds holobounds;
	public float scale = 2;
	public bool square = false;
	
	void Update(){
		if(holobounds!=null){
			transform.localScale=square?
				new Vector3(scale,1,scale):
				new Vector3(scale*holobounds.xRatio,scale,1);
		}
	}
}