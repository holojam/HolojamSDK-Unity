//UserManager.cs
//Created by Aaron C Gaudette on 22.06.16

using UnityEngine;

namespace Holojam{
	[ExecuteInEditMode]
	public class UserManager : MonoBehaviour{
		public TrackedHeadset user, actor;
		[Space(8)]
		public LiveObjectTag userTag = LiveObjectTag.HEADSET1;
		public int users = 4;
		
		void Update(){
			if(!Application.isPlaying)Start();
			users=Mathf.Max(users,1);
		}
		void Start(){Populate();}
		
		void Populate(){
			Clean();
			if(user==null || (actor==null && users>1))return;
			
			GameObject u = Instantiate(user.gameObject,Vector3.zero,Quaternion.identity) as GameObject;
			u.transform.parent=this.transform;
			u.name="User ("+((int)userTag+1)+")";
			u.GetComponent<TrackedHeadset>().liveObjectTag=userTag;
			
			for(int i=0;i<users;++i){
				if(i==(int)userTag)continue;
				GameObject a = Instantiate(actor.gameObject,Vector3.zero,Quaternion.identity) as GameObject;
				a.transform.parent=transform;
				a.name="Actor ("+(i+1)+")";
				a.GetComponent<TrackedHeadset>().liveObjectTag=(LiveObjectTag)i;
			}
		}
		void Clean(){
			foreach(TrackedHeadset t in GetComponentsInChildren<TrackedHeadset>(true))
				if(Application.isEditor && !Application.isPlaying)
					DestroyImmediate(t.gameObject);
				else Destroy(t.gameObject);
		}
	}
}