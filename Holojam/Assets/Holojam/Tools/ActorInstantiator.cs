//ActorInstantiator.cs
//Created by Aaron C Gaudette on 22.06.16

using UnityEngine;

namespace Holojam{
	[ExecuteInEditMode]
	public class ActorInstantiator : MonoBehaviour{
		public TrackedHeadset actor;
		public int amount = 4;
		
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
				a.GetComponent<TrackedHeadset>().liveObjectTag=(LiveObjectTag)GetComponent<ActorManager>().actors.Length+i;
			}
			GetComponent<ActorManager>().Update();
		}
		//Destroy all actors
		public void Clear(){
			foreach(TrackedHeadset a in GetComponent<ActorManager>().actors)
				if(Application.isEditor && !Application.isPlaying)
					DestroyImmediate(a.gameObject);
				else Destroy(a.gameObject);
		}
	}
}