//Stylus.cs
//Created on 23.06.16 by Aaron C Gaudette

using UnityEngine;

public class Stylus : MonoBehaviour{
	public Color color = Color.red;
	
	//Placeholder, will add functionality later
	Renderer model, makeLine;
	
	void Init(){
		if(model==null)model=transform.Find("Model").GetComponent<Renderer>();
		if(makeLine==null)makeLine=transform.Find("MakeLine").GetComponent<Renderer>();
	}
	void Update(){
		Init();
		model.material.color=color;
		makeLine.material.color=color;
	}
	
	void OnDrawGizmos(){
		Init();
		if(model==null)Update();
		Gizmos.color=color;
		Gizmos.DrawWireSphere(model.transform.position,0.06f);
	}
}