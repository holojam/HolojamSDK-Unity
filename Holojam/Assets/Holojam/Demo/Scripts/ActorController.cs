//ActorController.cs
//Created by Aaron C Gaudette on 05.07.16
//Example Actor type with separate head transform

using UnityEngine;

public class ActorController : Holojam.Actor{
	public Transform head;
	
	void Start(){ApplyMotif();}
	
	protected override void UpdateTracking(){
		if(!Application.isPlaying)return;
		
		if(view.IsTracked){
			transform.position=view.RawPosition;
			
			//This example uses a separate transform for rotation instead of itself
			if(head!=null){
				head.localPosition=Vector3.zero;
				head.rotation=view.RawRotation;
			}
			else Debug.LogWarning("ActorController: No head found for "+gameObject.name);
		}
	}
	public override Vector3 eyes{
		get{return transform.position;}
	}
	public override Quaternion orientation{
		get{return head!=null?head.rotation:Quaternion.identity;}
	}
	
	//Assign color of geometry with Motif tag
	void ApplyMotif(){
		if(Application.isPlaying)
			foreach(Renderer r in GetComponentsInChildren<Renderer>())
				if(r.gameObject.tag=="Motif")r.material.color=motif;
	}
}