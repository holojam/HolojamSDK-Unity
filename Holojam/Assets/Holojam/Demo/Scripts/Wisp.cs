//Wisp.cs
//Created by Aaron C Gaudette on 14.07.16

using UnityEngine;

public class Wisp : Holojam.Synchronizable{
	void Reset(){label="Wisp";}
	
	const float speed = 0.6f;
	
	protected override void Sync(){
		if(sending){
			
			transform.position+=new Vector3(
				Input.GetAxis("LJoystickY")*speed*Time.deltaTime,
				-Input.GetAxis("RJoystickY")*speed*Time.deltaTime,
				-Input.GetAxis("LJoystickX")*speed*Time.deltaTime
			);
			
			synchronizedVector3=transform.position;
			//synchronizedQuaternion=transform.rotation;
		}
		else{
			transform.position=synchronizedVector3;
			//transform.rotation=synchronizedQuaternion;
		}
	}
}