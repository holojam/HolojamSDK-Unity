// Global.cs
// Created by Holojam Inc. on 11.11.16

using UnityEngine;

namespace Holojam.Utility {

  /// <summary>
  /// Like a Singleton: enforces single instance, but no direct instance access
  /// (exposed through static functions and properties).
  /// </summary>
  /// <typeparam name="T"></typeparam>
  public abstract class Global<T> : MonoBehaviour where T : MonoBehaviour {
    static T t;
    static UnityEngine.Object lockObject = new UnityEngine.Object();

    protected static T global {
      get {
        lock (lockObject) {
          if (t == null) {
            T[] objects = FindObjectsOfType(typeof(T)) as T[];
            if (objects.Length == 0) {
              Debug.LogWarning("Global: " +
                 "No instances of " + typeof(T) + " in scene! (static access failure)"
              );
              return null;
            }
            t = objects[0];
            if (objects.Length > 1) {
              Debug.LogWarning("Global: " +
                 "More than one instance (" + objects.Length + ") of " + typeof(T) + " in scene! " +
                 "(expect undefined behavior)", t
              );
            }
          }
          return t;
        }
      }
    }
  }
}
