//ActorManager.cs
//Created by Aaron C Gaudette on 22.06.16

using UnityEngine;

namespace Holojam{
	[ExecuteInEditMode]
	public class ActorManager : MonoBehaviour{
		public Camera view; //VR camera for target device
		public GameObject shell; //Visual representation of other actors
		public LiveObjectTag buildTag = LiveObjectTag.HEADSET1; //Target device
		public bool runtimeIndexing = false;
		
		[HideInInspector] public TrackedHeadset[] actors; //Actor array reference
		LiveObjectTag[] tagCache;
		LiveObjectTag cachedBuildTag;
		
		public void Update(){
			if(!Application.isPlaying || runtimeIndexing)
				//Force index in case prefabs are updated (will increase logging!)
				Index(Application.isEditor && !Application.isPlaying);
		}
		
		void Index(bool force = false){
			if(actors.Length!=transform.childCount)actors=new TrackedHeadset[transform.childCount];
			LiveObjectTag[] tags = new LiveObjectTag[transform.childCount];
			
			bool equal = tagCache!=null && tagCache.Length==tags.Length;
			//Build actor array and cache
			for(int i=0;i<transform.childCount;++i){
				if(actors[i]==null)actors[i]=transform.GetChild(i).GetComponent<TrackedHeadset>();
				tags[i]=actors[i].liveObjectTag;
				equal=equal && tags[i]==tagCache[i];
			}
			//If tags differ from last check, perform index
			if(equal && buildTag==cachedBuildTag && !force)return;
			tagCache=tags;
			cachedBuildTag=buildTag;
			
			if(actors.Length==0){
				if(Application.isPlaying)Debug.LogWarning("ActorManager: No actors in hierarchy!");
				return;
			}
			if(view==null || (shell==null && actors.Length>1)){
				Debug.LogWarning("ActorManager: View/Shell prefab reference is null");
				return;
			}
			
			//Index each actor
			bool setView = false;
			foreach(TrackedHeadset a in actors){
				a.transform.position=Vector3.zero;
				a.transform.rotation=Quaternion.identity;
				
				//Flush view or shell
				foreach(Transform child in a.transform)
					if(child.name=="View" || child.name=="Shell")
						if(Application.isEditor && !Application.isPlaying)
							DestroyImmediate(child.gameObject);
						else Destroy(child.gameObject);
				
				//Is this the view actor?
				bool isView = a.liveObjectTag==buildTag;
				if(isView && setView){
					Debug.LogWarning("ActorManager: Duplicate build actor!");
					isView=false;
				}
				a.gameObject.name="Actor "+((int)a.liveObjectTag+1)+(isView?" (View)":"");
				
				//Create view or shell
				GameObject o = isView?
					Instantiate(view.gameObject,Vector3.zero,Quaternion.identity) as GameObject:
					Instantiate(shell.gameObject,Vector3.zero,Quaternion.identity) as GameObject;
				
				o.transform.parent=a.transform;
				o.name=isView?"View":"Shell";
				setView=setView || isView;
			}
			if(!setView)Debug.LogWarning("ActorManager: No actor found with matching build tag!");
		}
	}
}