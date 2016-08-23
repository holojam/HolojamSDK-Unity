//ActorManager.cs
//Created by Aaron C Gaudette on 22.06.16

using UnityEngine;
using Holojam.Network;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Holojam.Tools{
   [ExecuteInEditMode]
   public class ActorManager : MonoBehaviour{
      public Motive.Tag buildTag = Motive.Tag.HEADSET1;
      public Viewer viewer; //Viewer (headset tracker, VR camera)
      public bool previewMode = false; //Show all masks
      public bool runtimeIndexing = false;

      [HideInInspector] public Actor[] actors = new Actor[4]; //Actor array reference
      int[] indexCache;
      Motive.Tag cachedBuildTag;
      bool cachedPreviewMode = false;

      //Get the current build actor (re-index if necessary)
      [HideInInspector] public Actor ba;
      public Actor buildActor{get{
         if(ba==null && (!Application.isPlaying || runtimeIndexing))Index(true);
         return ba;
      }}

      void OnValidate(){Update();} //Make sure settings are saved before a build
      public void Update(){
         //Force index in case prefabs are updated (will increase logging!)
         if(!Application.isPlaying || runtimeIndexing)
            Index(Application.isEditor && !Application.isPlaying);
      }

      enum Result{INDEXED,PASSED,EMPTY,NOBUILD,NOVIEW};
      Result Index(bool force = false){
         int count = transform.GetComponentsInChildren<Actor>(true).Length;
         if(actors.Length!=count)actors = new Actor[count];
         int[] indices = new int[count];

         bool equal = indexCache!=null && indexCache.Length==indices.Length;
         //Build actor array and cache
         int index = 0;
         for(int i=0;i<transform.childCount;++i){
            Actor a = transform.GetChild(i).GetComponent<Actor>();
            if(a==null)continue; //Skip non-actor children
            if(actors[index]==null)actors[index] = a;

            indices[index] = actors[index].index; //Cache indices for comparison
            equal = equal && indices[index]==indexCache[index];
            index++;
         }

         //Index anyway on preview mode
         equal = equal && cachedPreviewMode!=previewMode;
         cachedPreviewMode = previewMode;

         //If tags differ from last check, perform index
         if(equal && buildTag==cachedBuildTag && !force)return Result.PASSED;
         indexCache = indices;
         cachedBuildTag = buildTag;

         if(actors.Length==0){
            if(Application.isPlaying)Debug.LogWarning("ActorManager: No actors in hierarchy!");
            return Result.EMPTY;
         }

         bool setBuild = false;
         //Index each actor
         foreach(Actor a in actors){
            //Is this the build actor?
            bool isBuild = a.trackingTag==buildTag;
            if(isBuild && setBuild){
               Debug.LogWarning("ActorManager: Duplicate build actor!");
               isBuild = false;
            } else if(isBuild)ba = a; //Assign reference
            a.gameObject.name="["+(a.index+1)+"] "+a.handle+(isBuild?" (Build)":"");

            //Activate mask
            if(a.mask!=null)a.mask.SetActive(previewMode || !isBuild);
            else Debug.Log("ActorManager: No mask found for Actor "+(a.index+1));

            setBuild=setBuild || isBuild;
         }
         if(!setBuild){
            Debug.LogWarning("ActorManager: No actor found with matching build tag!");
            return Result.NOBUILD;
         }
         //Update viewer
         if(viewer==null){
            Debug.LogWarning("ActorManager: Viewer prefab reference is null");
            return Result.NOVIEW;
         }
         else{
            viewer.actor=buildActor;
            #if UNITY_EDITOR
            EditorUtility.SetDirty(viewer);
            #endif
         }

         return Result.INDEXED;
      }
   }
}