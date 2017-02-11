//BuildManager.cs
//Created by Aaron C Gaudette on 11.11.16

using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Holojam.Tools {
  /// <summary>
  /// 
  /// </summary>
  [ExecuteInEditMode]
  public class BuildManager : Utility.Global<BuildManager> {

    /// <summary>
    /// 
    /// </summary>
    public Viewer viewer;

    /// <summary>
    /// 
    /// </summary>
    public enum Device {
      CARDBOARD, DAYDREAM, VIVE
    };

    /// <summary>
    /// 
    /// </summary>
    public const Device DEVICE_DEFAULT = Device.DAYDREAM;

    /// <summary>
    /// 
    /// </summary>
    public Device device = DEVICE_DEFAULT;

    /// <summary>
    /// 
    /// </summary>
    public static Device DEVICE {
      get { return global.device; }
    }

    /// <summary>
    /// 
    /// </summary>
    public bool preview = false;

    /// <summary>
    /// 
    /// </summary>
    public int previewIndex = 1;

    /// <summary>
    /// 
    /// </summary>
    public bool spectator = false;

    /// <summary>
    /// 
    /// </summary>
    public bool runtimeIndexing = true;

    int buildIndex = 0; //Defaults to master client
    /// <summary>
    /// 
    /// </summary>
    public static int BUILD_INDEX {
      get {
        return global.preview ? global.spectator ? 0 :
           global.previewIndex : global.buildIndex;
      }
      //Nothing implements this yet
      //set{global.buildIndex = value;}
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public static bool IsMasterClient() {
      return global && BUILD_INDEX == 0;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public static bool IsStandalone() {
      switch (Application.platform) {
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
    Actor buildActor; //Get the current build actor (re-index if necessary)

    /// <summary>
    /// 
    /// </summary>
    public Actor BuildActor {
      get {
        if (BUILD_INDEX == 0) {
          buildActor = null;
          return buildActor;
        }
        if (buildActor == null) {
          Index(true);
          return buildActor;
        }
        if (runtimeIndexing || !Application.isPlaying)
          Index();
        return buildActor;
      }
    }

    /// <summary>
    /// 
    /// </summary>
    public static Actor BUILD_ACTOR { get { return global.BuildActor; } }

    //Make sure settings are saved before a build
    bool loaded = false;
    void OnValidate() {
#if UNITY_EDITOR
      if (!loaded) return;
#endif
      Index(true);
      loaded = true;
    }

    enum Result { INDEXED, PASSED, EMPTY, NOBUILD, NOVIEW };
    Result Index(bool force = false) {
      int count = Actor.instances.Count;
      if (actors.Length != count) actors = new Actor[count];
      int[] indices = new int[count];

      bool equal = indexCache != null && indexCache.Length == indices.Length;
      //Build actor array and cache
      for (int i = 0; i < count; ++i) {
        actors[i] = Actor.instances[i];
        indices[i] = actors[i].index; //Cache indices for comparison
        equal = equal && indices[i] == indexCache[i];
      }

      //If tags differ from last check, perform index
      if (equal && BUILD_INDEX == cachedBuildIndex && !force) return Result.PASSED;
      indexCache = indices;
      cachedBuildIndex = BUILD_INDEX;

      if (actors.Length == 0)
        return Result.EMPTY;

      bool setBuild = false;
      //Index each actor
      foreach (Actor a in actors) {
        //Is this the build actor?
        bool isBuild = a.index == BUILD_INDEX;
        if (isBuild && setBuild) {
          Debug.LogWarning("BuildManager: Duplicate build actor!");
          isBuild = false;
        } else if (isBuild) buildActor = a; //Assign reference
        setBuild = setBuild || isBuild;
      }
      if (!setBuild && BUILD_INDEX != -1) {
        Debug.LogWarning("BuildManager: No actor found with matching build index!");
        buildActor = null;
        return Result.NOBUILD;
      }
      //Update viewer
      if (viewer == null) {
        Debug.LogWarning("BuildManager: Viewer prefab reference is null");
        return Result.NOVIEW;
      } else {
        viewer.actor = BuildActor;
#if UNITY_EDITOR
        EditorUtility.SetDirty(viewer);
#endif
      }

      return Result.INDEXED;
    }
  }
}
