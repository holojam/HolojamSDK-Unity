//HoloTime.cs
//Created by Aaron C Gaudette on 13.07.16

using UnityEngine;

public class HoloTime : Holojam.Tools.Synchronizable{
	public float time = 0;
	
	protected override void Sync(){
		if(sending){
			time+=Time.deltaTime;
			synchronizedVector3=new Vector3(time,0,0);
		}
		else time=synchronizedVector3.x;
	}
}