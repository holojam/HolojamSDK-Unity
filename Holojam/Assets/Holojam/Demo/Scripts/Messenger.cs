//Messenger.cs
//Created by Aaron C Gaudette on 13.07.16
//Peer-reviewed in REAL TIME by Daniel W. Zhang (Android Sysops)

using UnityEngine;

[RequireComponent(typeof(TextMesh))]
public class Messenger : Holojam.Synchronizable{
	void Reset(){label="Messenger";}
	
	public string tmp = "";
	
	public TextMesh text{get{
		if(textMesh==null)textMesh=GetComponent<TextMesh>();
		return textMesh;
	}}
	TextMesh textMesh = null;
	
	protected override void Sync(){
		if(sending){
			tmp+="a";
			synchronizedString=tmp;
		}
		else tmp=synchronizedString;
	}
}