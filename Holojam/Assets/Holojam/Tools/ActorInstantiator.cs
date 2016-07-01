//ActorInstantiator.cs
//Created by Aaron C Gaudette on 22.06.16
//Quickly add actors to the manager or destroy them

using UnityEngine;

namespace Holojam{
	[ExecuteInEditMode]
	[RequireComponent(typeof(ActorManager))]
	public class ActorInstantiator : MonoBehaviour{
		public Actor actor;
		public int amount = 4;
		
		Color[] colors = {
			Color.red,
			Color.green,
			Color.cyan,
			Color.yellow
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
				GameObject a = Instantiate(actor.gameObject,Vector3.zero,Quaternion.identity) as GameObject;
				a.transform.parent=transform;
				//Set tag and color automatically
				a.GetComponent<TrackedObject>().liveObjectTag=
					(LiveObjectTag)GetComponent<ActorManager>().actors.Length+i;
				if(a.GetComponent<Actor>())a.GetComponent<Actor>().color=colors[i%colors.Length];
			}
			GetComponent<ActorManager>().Update();
		}
		//Destroy all actors
		public void Clear(){
			foreach(Actor a in GetComponent<ActorManager>().actors)
				if(Application.isEditor && !Application.isPlaying)
					DestroyImmediate(a.gameObject);
				else Destroy(a.gameObject);
			GameObject v = GameObject.Find("Viewer");
			if(v!=null && Application.isEditor && !Application.isPlaying)
				DestroyImmediate(v); else if(v!=null)Destroy(v);
		}
	}
}