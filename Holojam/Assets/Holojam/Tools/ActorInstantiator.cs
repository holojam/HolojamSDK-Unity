//ActorInstantiator.cs
//Created by Aaron C Gaudette on 22.06.16
//Optional tool for easy setup and rapid prototyping

using UnityEngine;
using Holojam.Network;
using System;

namespace Holojam.Tools{
   [ExecuteInEditMode]
   [RequireComponent(typeof(ActorManager))]
   public class ActorInstantiator : MonoBehaviour{
      public Actor actor;
      public int amount = 4;

      //HoloIK-specific settings
      public Transform groundPlane;

      string[] handles = {
         "Blue",
         "Green",
         "Red",
         "Yellow"
      };
      Color[] colors = {
         new Color(0,0.5f,1),
         new Color(0.45f,1,0.45f),
         new Color(1,0,0.38f),
         new Color(1,0.84f,0),
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

            //HoloIK-specific settings
            HoloIK ik = a.GetComponentInChildren<HoloIK>();
            if(ik!=null)ik.groundPlane = groundPlane;
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