//BuildManager.cs
//Created by Aaron C Gaudette on 11.11.16

using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Holojam.Tools{
   [ExecuteInEditMode]
   public class BuildManager : Utility.Global<BuildManager>{
      public Viewer viewer;

      public bool preview = false;
      public int previewIndex;
      public bool runtimeIndexing = true;

      int buildIndex = 0;
      //Global
      public static int BUILD_INDEX{
         get{
            return global.preview?global.previewIndex:
               //Index -1 for spectator/ad-hoc server
               IsMasterPC()?-1:global.buildIndex;
         }
         //set{global.buildIndex = value;}
      }

      Actor[] actors = new Actor[0];
      int[] indexCache;
      int cachedBuildIndex;

      //Get the current build actor (re-index if necessary)
      Actor ba;
      public Actor buildActor{get{
         if(BUILD_INDEX==-1){
            ba = null;
            return ba;
         }
         if(ba==null && (!Application.isPlaying || runtimeIndexing))Index(true);
         return ba;
      }}
      public static Actor BUILD_ACTOR{get{return global.buildActor;}}

      //Make sure settings are saved before a build
      bool loaded = false;
      void OnValidate(){
         #if UNITY_EDITOR
         if(!loaded)return;
         #endif
         Index(true);
         loaded = true;
      }

      /*
      public void Update(){
         Force index in case prefabs are updated (will increase logging!)
         if(!Application.isPlaying || runtimeIndexing)
            Index(Application.isEditor && !Application.isPlaying);
      }
      */

      enum Result{INDEXED,PASSED,EMPTY,NOBUILD,NOVIEW};
      Result Index(bool force = false){
         int count = Actor.instances.Count;
         if(actors.Length!=count)actors = new Actor[count];
         int[] indices = new int[count];

         bool equal = indexCache!=null && indexCache.Length==indices.Length;
         //Build actor array and cache
         for(int i=0;i<count;++i){
            actors[i] = Actor.instances[i];
            indices[i] = actors[i].index; //Cache indices for comparison
            equal = equal && indices[i]==indexCache[i];
         }

         //If tags differ from last check, perform index
         if(equal && BUILD_INDEX==cachedBuildIndex && !force)return Result.PASSED;
         indexCache = indices;
         cachedBuildIndex = BUILD_INDEX;

         if(actors.Length==0)
            return Result.EMPTY;

         bool setBuild = false;
         //Index each actor
         foreach(Actor a in actors){
            //Is this the build actor?
            bool isBuild = a.index==BUILD_INDEX;
            if(isBuild && setBuild){
               Debug.LogWarning("BuildManager: Duplicate build actor!");
               isBuild = false;
            } else if(isBuild)ba = a; //Assign reference
            setBuild=setBuild || isBuild;
         }
         if(!setBuild && BUILD_INDEX!=-1){
            Debug.LogWarning("BuildManager: No actor found with matching build index!");
            ba = null;
            return Result.NOBUILD;
         }
         //Update viewer
         if(viewer==null){
            Debug.LogWarning("BuildManager: Viewer prefab reference is null");
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

      public static bool IsMasterPC(){
         switch(Application.platform){
            case RuntimePlatform.OSXEditor: return true;
            case RuntimePlatform.OSXPlayer: return true;
            case RuntimePlatform.WindowsEditor: return true;
            case RuntimePlatform.WindowsPlayer: return true;
            case RuntimePlatform.LinuxPlayer: return true;
         }
         return false;
      }
   }
}
