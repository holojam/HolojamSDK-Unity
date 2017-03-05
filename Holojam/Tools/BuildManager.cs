// BuildManager.cs
// Created by Holojam Inc. on 11.11.16

using System.Collections.ObjectModel;
using UnityEngine;

namespace Holojam.Tools {

  /// <summary>
  /// Indexes actors in the scene and manages the build target.
  /// </summary>
  [ExecuteInEditMode]
  public sealed class BuildManager : Utility.Global<BuildManager> {

    public enum Device {
      CARDBOARD, DAYDREAM, VIVE
    };

    /// <summary>
    /// Default build device in the editor.
    /// </summary>
    public const Device DEVICE_DEFAULT = Device.DAYDREAM;

    /// <summary>
    /// Target build device.
    /// </summary>
    public Device device = DEVICE_DEFAULT;

    /// <summary>
    /// Global build device accessor.
    /// </summary>
    public static Device DEVICE {
      get { return global.device; }
    }

    /// <summary>
    /// With preview mode turned on, Unity will simulate a build to the
    /// provided preview index.
    /// </summary>
    public bool preview = false;

    /// <summary>
    /// Overrides the build index during preview mode.
    /// </summary>
    public int previewIndex = 1;

    /// <summary>
    /// With spectator mode turned on (only available during preview mode)
    /// Unity will preview a specific Actor while still remaining a master client.
    /// </summary>
    public bool spectator = false;

    /// <summary>
    /// With runtime indexing set to true, the Build Manager will constantly
    /// re-index regardless of the cache.
    /// </summary>
    public bool runtimeIndexing = true;

    int buildIndex = 0; // Defaults to master client

    /// <summary>
    /// Global function returning the current build target index
    /// (or zero if the machine is a master client). This value defaults
    /// to zero.
    /// </summary>
    /// <returns>The current index of the Build Manager.</returns>
    public static int BUILD_INDEX {
      get {
        return global.preview ? global.previewIndex : global.buildIndex;
      }
      set { global.buildIndex = value; }
    }

    /// <summary>
    /// Is this a master client and not a targeted build?
    /// </summary>
    /// <returns>True if the machine is a master client or spectating.</returns>
    public static bool IsMasterClient() {
      return global && (BUILD_INDEX == 0 || global.spectator);
    }

    /// <summary>
    /// Is this an editor/standalone build?
    /// </summary>
    /// <returns>True if the machine is running on a desktop environment.</returns>
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
    Actor buildActor;

    /// <summary>
    /// Determine the build Actor, re-indexing if necessary.
    /// </summary>
    Actor BuildActor {
      get {
        if (BUILD_INDEX == 0) { // Don't use IsMasterClient() here
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
    /// Global accessor for the current build Actor.
    /// </summary>
    public static Actor BUILD_ACTOR { get { return global.BuildActor; } }

    // Make sure settings are saved before a build
    bool loaded = false;
    void OnValidate() {
      #if UNITY_EDITOR
        if (!loaded) return;
      #endif
      Index(true);
      loaded = true;
    }

    enum Result { INDEXED, PASSED, EMPTY, NOBUILD };

    /// <summary>
    /// Intelligently index the Actors in the scene, determine the build Actor,
    /// update the Viewer.
    /// </summary>
    Result Index(bool force = false) {
      ReadOnlyCollection<Actor> allActors = Network.Controller.All<Actor>();

      int count = allActors.Count;
      if (actors.Length != count) actors = new Actor[count];
      int[] indices = new int[count];

      bool equal = indexCache != null && indexCache.Length == indices.Length;
      // Build actor array and cache
      for (int i = 0; i < count; ++i) {
        actors[i] = allActors[i];
        indices[i] = actors[i].index; // Cache indices for comparison
        equal = equal && indices[i] == indexCache[i];
      }

      // If tags differ from last check, perform index
      if (equal && BUILD_INDEX == cachedBuildIndex && !force) return Result.PASSED;
      indexCache = indices;
      cachedBuildIndex = BUILD_INDEX;

      if (actors.Length == 0)
        return Result.EMPTY;

      bool setBuild = false;
      // Index each actor
      foreach (Actor a in actors) {
        // Is this the build actor?
        bool isBuild = a.index == BUILD_INDEX;
        if (isBuild && setBuild) {
          Debug.LogWarning("BuildManager: Duplicate build actor!");
          isBuild = false;
        } else if (isBuild) buildActor = a; // Assign reference
        setBuild = setBuild || isBuild;
      }
      if (!setBuild && BUILD_INDEX != -1) {
        Debug.LogWarning("BuildManager: No actor found with matching build index!");
        buildActor = null;
        return Result.NOBUILD;
      }

      return Result.INDEXED;
    }
  }
}
