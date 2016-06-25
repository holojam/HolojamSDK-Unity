//PairableManager.cs
//Created by Aaron C Gaudette on 24.06.16

using UnityEngine;

namespace Holojam{
	[ExecuteInEditMode]
	public class PairableManager : MonoBehaviour{
		public float pairRange = 0.22f;
		public float pairDistance = 0.25f;
		public float pairTime = 1.5f;
		public float cooldown = 1;
		
		[Space(8)]
		public Transform pairingSphere;
		public ActorManager actorManager;
		[HideInInspector] public Actor[] actors = new Actor[0];
		[HideInInspector] public Pairable[] pairables = new Pairable[0];
		
		void Update(){
			if(actorManager!=null)actors=actorManager.actors;
			else Debug.LogWarning("PairableManager: ActorManager not set!");
			if(pairables.Length!=transform.childCount)pairables=new Pairable[transform.childCount];
			
			pairingSphere.GetComponent<Renderer>().enabled=false; //Assume the sphere will not be activated
			for(int i=0;i<transform.childCount;++i){
				if(pairables[i]==null)pairables[i]=transform.GetChild(i).GetComponent<Pairable>();
				
				pairables[i].gameObject.name=pairables[i].type+" "+
					(pairables[i].paired?(pairables[i].pairedActor.index+1).ToString():"(Unpaired)");
				//Make a pairing check for each actor on each pairable
				if(Application.isPlaying)foreach(Actor a in actors)CheckPair(pairables[i],a);
			}
		}
		void CheckPair(Pairable g, Actor a){
			if(Time.time-g.lastPaired<cooldown)return;
			
			//Check within a sphere
			bool inRange = Vector3.Distance(
				g.center,a.transform.position+a.transform.forward*pairDistance
			)<=0.5f*pairRange;
			bool isTarget = g.IsTarget(a); //Is this actor currently attempting to pair?
			bool paired = g.paired;// || Paired(a); //Uncomment for pair limits
			bool thisPaired = g.pairedActor==a; //Is this pairable the actor's current pairable?
			
			//Render pairing sphere
			if(a==actorManager.buildActor && isTarget && inRange){
				pairingSphere.localScale=pairRange*Vector3.one;
				pairingSphere.position=a.transform.position+a.transform.forward*pairDistance;
				pairingSphere.GetComponent<Renderer>().enabled=true;
			}
			
			//Debug visualization
			float tick = 0.1f*(Time.time%1);
			if(isTarget){
				//Swap colors if unpairing
				Debug.DrawLine(g.center,a.transform.position,thisPaired?a.color:Color.white);
				//Draw animated axis
				Vector3 progress = Vector3.Lerp(g.center,a.transform.position,g.PairTime(a)/pairTime);
				Debug.DrawLine(progress+Vector3.up*tick,progress-Vector3.up*tick,thisPaired?Color.white:a.color);
				Debug.DrawLine(progress+Vector3.left*tick,progress-Vector3.left*tick,thisPaired?Color.white:a.color);
				Debug.DrawLine(progress+Vector3.forward*tick,progress-Vector3.forward*tick,thisPaired?Color.white:a.color);
			}
			else if(thisPaired)Debug.DrawLine(g.center,a.transform.position,a.color);
			else if(inRange)Debug.DrawLine(g.center,a.transform.position,Color.gray);
			
			if(isTarget && ((paired && !thisPaired) || !inRange)) //Remove if paired / remove if out of range
				g.RemoveTarget(a);
			else if(!isTarget && inRange && (!paired || thisPaired)) //Check for unpair / add if unpaired
				g.AddTarget(a,Time.time);
			
			//Pair
			isTarget=g.IsTarget(a); //Recheck
			if(isTarget && g.PairTime(a)>=pairTime)
				Pair(g,thisPaired?null:a);
		}
		void Pair(Pairable g, Actor a){
			g.pairedActor=a;
			g.ApplyMotif(a!=null?a.color:Color.gray);
			g.ClearTargets();
			g.lastPaired=Time.time;
			Debug.Log(a!=null?
				"PairableManager: Pairable "+g.GetComponent<TrackedObject>().liveObjectTag+" paired with "+a.gameObject.name:
				"PairableManager: Pairable "+g.GetComponent<TrackedObject>().liveObjectTag+" unpaired"
			);
		}
		
		bool Paired(Actor a){
			foreach(Pairable g in pairables)if(g.pairedActor==a)return true;
			return false;
		}
	}
}