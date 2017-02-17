// Notifier.cs
// Created by Holojam Inc. on 16.01.17

using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using System.Collections;
#endif

namespace Holojam.Network {

  /// <summary>
  /// Global Holojam event system manager.
  /// </summary>
  public class Notifier : Utility.Global<Notifier> {

    // Single-instance, so this is ok
    Dictionary<string, List<Callback>> subscriptions = new Dictionary<string, List<Callback>>();

    public delegate void Callback(string source, Flake data);

    // Debug
    #if UNITY_EDITOR
    public const float FIRE_TIME = 1;
    public List<string> eventData = new List<string>();
    #endif

    /// <summary>
    /// Add a callback to the subscriptions table.
    /// Recommended usage: call from the class with the callback.
    /// </summary>
    /// <param name="callback">A function to call on event trigger.</param>
    /// <param name="label">Holojam event label to subscribe to.</param>
    /// <param name="scope">
    /// Namespace to filter. Default: empty string; pass an empty string to listen to all.
    /// </param>
    public static void AddSubscriber(Callback callback, string label, string scope = "") {
      if (!Application.isPlaying) return;

      string brand = scope + "." + label;
      if (!global.subscriptions.ContainsKey(brand))
        global.subscriptions[brand] = new List<Callback>();
      global.subscriptions[brand].Add(callback);
    }

    /// <summary>
    /// Remove a callback from the subscriptions table.
    /// Recommended usage: call from the class with the callback on destruction.
    /// </summary>
    /// <param name="callback"></param>
    /// <param name="label"></param>
    /// <param name="scope"></param>
    /// <returns>True if a subscription with the specified label existed.</returns>
    public static bool RemoveSubscriber(Callback callback, string label, string scope = "") {
      if (!Application.isPlaying) return false;

      string brand = scope + "." + label;
      if (!global.subscriptions.ContainsKey(brand))
        return false;
      int i = global.subscriptions[brand].IndexOf(callback);
      if (i < 0) return false;
      global.subscriptions[brand].RemoveAt(i);
      if (global.subscriptions[brand].Count == 0)
        global.subscriptions.Remove(brand);
      return true;
    }

    /// <summary>
    /// Publish a Holojam event from the network/broadcast it to subscribers, if there are any.
    /// Does nothing if no subscribers care about the specified event.
    /// </summary>
    /// <param name="e"></param>
    static internal void Publish(Event e) {
      string brand = e.scope + "." + e.label;
      string unscoped = "." + e.label; // Also call any subscribers to this label without scope

      #if UNITY_EDITOR
      global.StartCoroutine(global.FireDebug(brand + " (" + e.source + ")"));
      #endif

      bool specific = global.subscriptions.ContainsKey(brand);
      bool general = global.subscriptions.ContainsKey(unscoped);

      if (!specific && !general)
        return;

      Flake flake = e.notification? null : new Flake();
      e.Load(flake);

      if (specific)
        foreach (Callback callback in global.subscriptions[brand])
          callback(e.source, flake);

      if (general)
        foreach (Callback callback in global.subscriptions[unscoped])
          callback(e.source, flake);
    }

    #if UNITY_EDITOR
    IEnumerator FireDebug(string e) {
      eventData.Add(e);
      yield return new WaitForSeconds(FIRE_TIME);
      eventData.Remove(e);
    }
    #endif
  }
}
