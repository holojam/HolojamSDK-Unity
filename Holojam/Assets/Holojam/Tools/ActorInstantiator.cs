//ActorInstantiator.cs
//Created by Aaron C Gaudette on 22.06.16
//Optional tool for easy setup and rapid prototyping

using UnityEngine;
using Holojam.Network;
using System;

namespace Holojam{
	[ExecuteInEditMode]
	[RequireComponent(typeof(ActorManager))]
	public class ActorInstantiator : MonoBehaviour{
		public Actor actor;
		public int amount = 4;
		
		string[] handles = {
			"Blue",
			"Green",
			"Red",
			"Yellow"
		};
		Color[] colors = {
			Color.cyan,
			Color.green,
			Color.red,
			Color.yellow
		};
		
		public void Add(){
			if(actor==null){
				Debug.LogWarning("ActorInstantiator: Actor reference is null!");
				return;
			}
			for(int i=0;i<amount;++i){
				Actor a = (Instantiate(actor.gameObject,Vector3.zero,Quaternion.identity) as GameObject).GetComponent<Actor>();
				a.transform.parent=transform;
				//Set tag and color automatically
				int index = (i+GetComponent<ActorManager>().actors.Length)%Motive.tagCount;
				a.trackingTag=(Motive.Tag)index;
				a.handle=handles[i%handles.Length];
				a.motif=colors[i%colors.Length];
			}
			GetComponent<ActorManager>().Update();
		}
		//Destroy all actors
		public void Clear(){
			foreach(Actor a in GetComponent<ActorManager>().actors)
				if(Application.isEditor && !Application.isPlaying)
					DestroyImmediate(a.gameObject);
				else Destroy(a.gameObject);
		}
	}
}