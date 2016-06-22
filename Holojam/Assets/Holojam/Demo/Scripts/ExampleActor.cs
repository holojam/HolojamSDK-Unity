//ExampleActor.cs
//Created by Aaron C Gaudette on 22.06.16

using UnityEngine;
[RequireComponent(typeof(Holojam.TrackedHeadset))]

public class ExampleActor : MonoBehaviour{
	//Index of actor (which headset is this?)
	int actorIndex{get{return (int)(GetComponent<Holojam.TrackedHeadset>().liveObjectTag);}}
	
	Renderer hat;
	public Material[] hatColors = new Material[4];
	
	void Start(){
		hat=transform.GetChild(1).GetComponent<Renderer>();
	}
	void Update(){
		//Display a different color hat for each player, consistent across each user (build)
		hat.material=hatColors[actorIndex];
	}
}