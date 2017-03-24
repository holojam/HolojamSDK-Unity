// EventListener.cs
// Created by Holojam Inc. on 16.02.17

#if UNITY_EDITOR
using System.Collections;
using UnityEditor;
#endif

using UnityEngine;

namespace Holojam.Network {

  /// <summary>
  /// Abstract base class (boilerplate) for subscribing to Holojam events in Unity.
  /// </summary>
  public abstract class EventListener : MonoBehaviour, IEvent {

    #if UNITY_EDITOR
    [SerializeField] internal View view;
    #endif

    /// <summary>
    /// Filter (string namespace) for incoming events.
    /// An empty scope is parsed as a "whitelist all".
    /// </summary>
    public virtual string Scope { get { return ""; } }

    /// <summary>
    /// Label (string) of the event to subscribe to.
    /// Call UpdateSubscription() when changing this.
    /// </summary>
    public abstract string Label { get; }

    public string Brand { get { return Scope + "." + Label; } }

    string lastLabel;

    /// <summary>
    /// Update the subscription to the latest Label.
    /// </summary>
    public void UpdateSubscription() {
      Unsubscribe();
      lastLabel = Label;
      Subscribe();
      #if UNITY_EDITOR
      EditorUtility.SetDirty((UnityEngine.Object)this);
      #endif
    }

    /// <summary>
    /// Unsubscribe from the Notifier with this listener's data.
    /// </summary>
    public void Unsubscribe() {
      Notifier.RemoveSubscriber(Intercept, lastLabel, Scope);
    }

    /// <summary>
    /// Subscribe to the Notifier with this listener's data.
    /// </summary>
    void Subscribe() {
      Notifier.AddSubscriber(Intercept, lastLabel, Scope);
    }

    /// <summary>
    /// Override this to specify the function called on event trigger.
    /// </summary>
    protected abstract Notifier.Callback Callback { get; }

    /// <summary>
    /// Subscription management. Must call base.OnEnable() if overriding.
    /// </summary>
    protected virtual void OnEnable() {
      UpdateSubscription();
    }

    /// <summary>
    /// Subscription management. Must call base.OnDisable() if overriding.
    /// </summary>
    protected virtual void OnDisable() {
      Unsubscribe();
    }

    #if UNITY_EDITOR
    public bool Fired { get { return fired; } }
    bool fired = false;
    #endif

    /// <summary>
    /// Intercept function for display (View) purposes in the editor.
    /// </summary>
    void Intercept(string source, string scope, Flake input) {
      #if UNITY_EDITOR
      StartCoroutine(Fire());
      #endif
      Callback(source, scope, input);
    }

    #if UNITY_EDITOR
    IEnumerator Fire() {
      if (fired) yield return null;
      EditorUtility.SetDirty((UnityEngine.Object)this);
      fired = true;
      yield return new WaitForSeconds(Notifier.EDITOR_FIRE_TIME);
      fired = false;
      EditorUtility.SetDirty((UnityEngine.Object)this);
    }
    #endif
  }
}
