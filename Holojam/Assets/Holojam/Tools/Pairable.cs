//Pairable.cs
//Created by Aaron C Gaudette on 24.06.16

using UnityEngine;
using System.Collections.Generic;

namespace Holojam{
	[ExecuteInEditMode]
	[RequireComponent(typeof(TrackedObject))]
	public class Pairable : MonoBehaviour{
		public string type = "Grip";
		public Vector3 offset = Vector3.zero; //Manual offset, useful for soft trackers
		public Vector3 center{get{return transform.TransformPoint(offset);}}
		
		public Actor pairedActor = null;
		public bool paired{get{return pairedActor!=null;}}
		public bool pairing{get{return pairTargets.Count>0 && !paired;}}
		public bool unpairing{get{return pairTargets.Count>0 && paired;}}
		[HideInInspector] public float lastPaired = 0;
		
		//Dictionary for pairing attempts
		Dictionary<Actor,float> pairTargets = new Dictionary<Actor,float>();
		public void AddTarget(Actor a, float t){pairTargets.Add(a,t);}
		public void RemoveTarget(Actor a){pairTargets.Remove(a);}
		public void ClearTargets(){pairTargets.Clear();}
		
		public bool IsTarget(Actor a){return pairTargets.ContainsKey(a);}
		public float PairTime(Actor a){return Time.time-pairTargets[a];} //Pair attempt lifetime
		
		//Color
		public void ApplyMotif(Color c){
			if(Application.isPlaying)
				foreach(Renderer r in GetComponentsInChildren<Renderer>()){
					if(r.gameObject.tag=="Motif")r.material.color=c;
					r.enabled=true;
				}
		}
		
		void Update(){
			//Pairing blink effect
			foreach(Renderer r in GetComponentsInChildren<Renderer>())
				r.enabled=(pairing || unpairing)?Time.time%0.2f>0.1f:true;
		}
		
		void OnDrawGizmos(){
			//Pivot
			Gizmos.color=Color.white;
			Gizmos.DrawLine(transform.position-0.02f*Vector3.up,transform.position+0.02f*Vector3.up);
			Gizmos.DrawLine(transform.position-0.02f*Vector3.left,transform.position+0.02f*Vector3.left);
			Gizmos.DrawLine(transform.position-0.02f*Vector3.forward,transform.position+0.02f*Vector3.forward);
			//Offset
			Gizmos.color=paired?pairedActor.color:Color.gray;
			Gizmos.DrawWireSphere(center,0.06f);
		}
	}
}