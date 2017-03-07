// EventPusher.cs
// Created by Holojam Inc. 16.02.17

#if UNITY_EDITOR
using System.Collections;
using UnityEditor;
#endif

using UnityEngine;

namespace Holojam.Network {

  /// <summary>
  /// Abstract base class (boilerplate) for pushing Holojam events in Unity.
  /// Contains a Network.Flake. Extend to implement custom functionality. See examples and Flake.cs.
  /// </summary>
  public abstract class EventPusher : FlakeComponent, IEvent {

    #if UNITY_EDITOR
    [SerializeField] internal View view;
    #endif

    /// <summary>
    /// Label (string) to push the data under.
    /// </summary>
    public abstract string Label { get; }

    public string Brand { get { return Client.SEND_SCOPE + "." + Label; } }

    #if UNITY_EDITOR
    public bool Fired { get { return fired; } }
    bool fired = false;
    #endif

    /// <summary>
    /// Push an event with the current Label and data.
    /// </summary>
    public void Push() {
      Client.PushEvent(Label, data);
      #if UNITY_EDITOR
      StartCoroutine(Fire());
      #endif
    }

    #if UNITY_EDITOR
    IEnumerator Fire() {
      if (fired) yield return null;
      fired = true;
      yield return new WaitForSeconds(Notifier.EDITOR_FIRE_TIME);
      fired = false;
    }
    #endif

    /// <summary>
    /// Overriden for display (View) purposes in the editor.
    /// Must call base.LateUpdate() if overriding.
    /// </summary>
    protected virtual void LateUpdate() {
      #if UNITY_EDITOR
      EditorUtility.SetDirty((UnityEngine.Object)this);
      #endif
    }
  }
}
