//Stylus.cs
//Created on 23.06.16 by Aaron C Gaudette
//Merged with LineUnity.cs on 10.07.16

using UnityEngine;
using System.Collections.Generic;
using Holojam.IO;
using Holojam.Network;

public class Stylus : MonoBehaviour{
	public Color color = Color.white;
	public makeLine line;
	public Renderer model;
	public Transform tip;
	
	const int maxVertexCount = 1024;
	List<makeLine> lines = new List<makeLine>();
	
	bool drawing = false, erasing = false, click = false;
	bool added = true;
	
	void Update(){
		model.material.color=color;
		line.GetComponent<Renderer>().material.color=color;
		
		OnDrag();
		
		int bits = GetComponent<HolojamView>().Bits;
		
		if((bits & ButtonConstants.A)>0){
			if(!drawing){
				OnButtonA();
				drawing=true;
			}
		} else if(drawing){
			OnRelease();
			drawing=false;
		}
		
		if((bits & ButtonConstants.B)>0){
			if(!erasing){
				OnButtonB();
				erasing=true;
			}
		} else if(erasing){
			OnRelease();
			erasing=false;
		}
	}
	
	public void OnButtonA(){
		click=!drawing;
		drawing=true;
	}
	public void OnButtonB(){erasing=true;}
	public void OnRelease(){
		drawing=false;
		erasing=false;
		click=false;
		added=true;
	}
	
	public void OnDrag(){
		if(added && click){
			AddLine();
			click=false;
			added=false;
		}
		if(drawing)DrawLine(drawing);
		else if(erasing)EraseLine(drawing);
	}
	
	public void DrawLine(bool drawing){
		lines[lines.Count-1].addPoints(tip.gameObject,drawing);
	}
	public void EraseLine(bool drawing){
		for(int i=0;i<lines.Count;++i)
			lines[i].addPoints(tip.gameObject,drawing);
	}
	public void AddLine(){
		if(lines.Count<1)lines.Add(Instantiate(line) as makeLine);
		lines[0].addNewLine(tip.gameObject);
		lines[0].maxPoints=maxVertexCount;
	}
	
	void OnDrawGizmos(){
		if(tip==null)return;
		Gizmos.color=color;
		Gizmos.DrawWireSphere(tip.position,0.04f);
	}
}