//ExampleActor.cs
//Created by Aaron C Gaudette on 22.06.16

using UnityEngine;
[RequireComponent(typeof(Holojam.TrackedHeadset))]

public class ExampleActor : MonoBehaviour{
	//Which headset is this?
	int actorIndex{get{return (int)(GetComponent<Holojam.TrackedHeadset>().liveObjectTag);}}
	bool isView{get{return actorIndex==(int)am.buildTag;}}
	
	Holojam.ActorManager am; //Dependent on ActorManager
	Renderer hat;
	public Material[] hatColors = new Material[4];
	
	void Update(){
		if(am==null && transform.parent.GetComponent<Holojam.ActorManager>()!=null)
			am=transform.parent.GetComponent<Holojam.ActorManager>();
		if(!isView){
			Transform shell = transform.Find("Shell");
			if(shell!=null){
				hat=shell.GetChild(1).GetComponent<Renderer>();
				//Display a different color hat for each player, consistent for each build
				hat.material=hatColors[actorIndex];
			}
		}
	}
}