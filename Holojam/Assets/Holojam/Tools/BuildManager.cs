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

      public enum Device{
         CARDBOARD,DAYDREAM,VIVE
      };
      public const Device DEVICE_DEFAULT = Device.DAYDREAM;
      public Device device = DEVICE_DEFAULT;
      public static Device DEVICE{
         get{return global.device;}
      }

      public bool preview = false;
      public int previewIndex = 1;
      public bool spectator = false;
      public bool runtimeIndexing = true;

      int buildIndex = 0; //Defaults to master client
      public static int BUILD_INDEX{
         get{
            return global.preview? global.previewIndex:
               global.spectator? 0:global.buildIndex;
         }
         //Nothing implements this yet
         //set{global.buildIndex = value;}
      }

      public static bool IsMasterClient(){
         return global && BUILD_INDEX==0;
      }

      public static bool IsStandalone(){
         switch(Application.platform){
            case RuntimePlatform.OSXEditor: return true;
            case RuntimePlatform.OSXPlayer: return true;
            case RuntimePlatform.WindowsEditor: return true;
            case RuntimePlatform.WindowsPlayer: return true;
            case RuntimePlatform.LinuxPlayer: return true;
         }
         return false;
      }

      Actor[] actors = new Actor[0];
      int[] indexCache;
      int cachedBuildIndex;

      //Get the current build actor (re-index if necessary)
      Actor ba;
      public Actor buildActor{get{
         if(BUILD_INDEX==0){
            ba = null;
            return ba;
         }
         if(ba==null){
            Index(true);
            return ba;
         }
         if(runtimeIndexing || !Application.isPlaying)
            Index();
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
   }
}
