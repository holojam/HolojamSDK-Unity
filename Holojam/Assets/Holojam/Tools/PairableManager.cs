//PairableManager.cs
//Created by Aaron C Gaudette on 24.06.16

using UnityEngine;

namespace Holojam{
	[ExecuteInEditMode]
	public class PairableManager : MonoBehaviour{
		public ActorManager actorManager;
		public GameObject pairingSphere;
		
		[System.Serializable]
		public class Pairing{
			public float diameter = 0.16f;
			public float distance = 0.18f;
			public float time = 1.5f;
			public float cooldown = 1;
		}
		public Pairing pair;
		
		[HideInInspector] public Actor[] actors = new Actor[0];
		[HideInInspector] public Pairable[] pairables = new Pairable[0];
		Transform sphere;
		
		void Update(){
			if(actorManager!=null)actors=actorManager.actors;
			else Debug.LogWarning("PairableManager: ActorManager not set!");
			if(pairables.Length!=transform.childCount)pairables=new Pairable[transform.childCount];
			
			if(pairingSphere!=null && Application.isPlaying){
				if(sphere==null)
					sphere=(Instantiate(pairingSphere,Vector3.zero,Quaternion.identity) as GameObject).transform;
				sphere.GetComponent<Renderer>().enabled=false; //Assume the sphere will not be activated
			}
			else if(pairingSphere==null){
				Debug.LogWarning("PairableManager: Pairing sphere not set!");
				return;
			}
			
			for(int i=0;i<transform.childCount;++i){
				if(pairables[i]==null)pairables[i]=transform.GetChild(i).GetComponent<Pairable>();
				
				pairables[i].gameObject.name=pairables[i].type+" "+
					(pairables[i].paired?(pairables[i].pairedActor.index+1).ToString():"(Unpaired)");
				//Make a pairing check for each actor on each pairable
				if(Application.isPlaying)foreach(Actor a in actors)CheckPair(pairables[i],a);
			}
		}
		void CheckPair(Pairable g, Actor a){
			if(Time.time-g.lastPaired<pair.cooldown)return;
			
			//Check within a sphere
			bool inRange = Vector3.Distance(
				g.center,a.eyes+a.look*pair.distance
			)<=0.5f*pair.diameter;
			bool isTarget = g.IsTarget(a); //Is this actor currently attempting to pair?
			bool paired = g.paired;// || Paired(a); //Uncomment for pair limits
			bool thisPaired = g.pairedActor==a; //Is this pairable the actor's current pairable?
			
			//Render pairing sphere
			if(a==actorManager.buildActor && isTarget && inRange){
				sphere.localScale=pair.diameter*Vector3.one;
				sphere.position=a.eyes+a.look*pair.distance;
				sphere.GetComponent<Renderer>().enabled=true;
			}
			
			//Debug visualization
			float tick = 0.1f*(Time.time%1);
			if(isTarget){
				//Swap colors if unpairing
				Debug.DrawLine(g.center,a.eyes,thisPaired?a.motif:Color.white);
				//Draw animated axis
				Vector3 progress = Vector3.Lerp(g.center,a.eyes,g.PairTime(a)/pair.time);
				Debug.DrawLine(progress+Vector3.up*tick,progress-Vector3.up*tick,thisPaired?Color.white:a.motif);
				Debug.DrawLine(progress+Vector3.left*tick,progress-Vector3.left*tick,thisPaired?Color.white:a.motif);
				Debug.DrawLine(progress+Vector3.forward*tick,progress-Vector3.forward*tick,thisPaired?Color.white:a.motif);
			}
			else if(thisPaired)Debug.DrawLine(g.center,a.eyes,a.motif);
			else if(inRange)Debug.DrawLine(g.center,a.eyes,Color.gray);
			
			if(isTarget && ((paired && !thisPaired) || !inRange)) //Remove if paired / remove if out of range
				g.RemoveTarget(a);
			else if(!isTarget && inRange && (!paired || thisPaired)) //Check for unpair / add if unpaired
				g.AddTarget(a,Time.time);
			
			//Pair
			isTarget=g.IsTarget(a); //Recheck
			if(isTarget && g.PairTime(a)>=pair.time)
				Pair(g,thisPaired?null:a);
		}
		void Pair(Pairable g, Actor a){
			g.pairedActor=a;
			g.ApplyMotif(a!=null?a.motif:Color.gray);
			g.ClearTargets();
			g.lastPaired=Time.time;
			Debug.Log(a!=null?
				"PairableManager: Pairable "+g.trackingTag+
					" paired with \""+a.gameObject.name+"\"":
				"PairableManager: Pairable "+g.trackingTag+" unpaired"
			);
		}
		
		bool Paired(Actor a){
			foreach(Pairable g in pairables)if(g.pairedActor==a)return true;
			return false;
		}
	}
}