//Messenger.cs
//Created by Aaron C Gaudette on 13.07.16

using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(TextMesh))]
public class Messenger : Holojam.Tools.Synchronizable{
	void Reset(){label="Messenger";}
	
	[Space(8)] public string handle = "";
	List<string> messages = new List<string>();
	
	const int maxMessages = 8;
	
	public TextMesh text{get{
		if(textMesh==null)textMesh=GetComponent<TextMesh>();
		return textMesh;
	}}
	TextMesh textMesh = null;
	
	protected override void Sync(){
		if(sending){
			text.text="";
			foreach(string m in messages)text.text+=m;
			synchronizedString=text.text;
		}
		else text.text=synchronizedString;
	}
	
	public void Push(string message){
		if(!sending)return;
		messages.Add(handle+": "+message+"\n");
		if(messages.Count>maxMessages)messages.RemoveAt(0);
	}
}