//ActorInstantiator.cs
//Created by Aaron C Gaudette on 22.06.16
//Optional tool for easy setup and rapid prototyping

using UnityEngine;
using Holojam.Server;

namespace Holojam{
	[ExecuteInEditMode]
	[RequireComponent(typeof(ActorManager))]
	public class ActorInstantiator : MonoBehaviour{
		public Actor actor;
		public int amount = 4;
		
		string[] handles = {
			"Red One",
			"Green Two",
			"Blue Three",
			"Yellow Four"
		};
		Color[] colors = {
			Color.red,
			Color.green,
			Color.cyan,
			Color.yellow
		};
		
		//Temporary fix until new labeling system is implemented
		string[] labels = {
			"VR1",
			"VR2",
			"VR3",
			"VR4"
		};
		
		void Update(){
			amount=Mathf.Max(amount,1);
		}
		public void Add(){
			if(actor==null){
				Debug.LogWarning("ActorInstantiator: Actor reference is null!");
				return;
			}
			for(int i=0;i<amount;++i){
				Actor a = (Instantiate(actor.gameObject,Vector3.zero,Quaternion.identity) as GameObject).GetComponent<Actor>();
				a.transform.parent=transform;
				//Set tag and color automatically
				a.view.label=labels[i%labels.Length];
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