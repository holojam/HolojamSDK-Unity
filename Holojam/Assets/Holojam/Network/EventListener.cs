// EventListener.cs
// Created by Holojam Inc. on 16.02.17

using UnityEngine;

namespace Holojam.Network {

  /// <summary>
  /// Abstract base class (boilerplate) for subscribing to Holojam events in Unity.
  /// </summary>
  public abstract class EventListener : MonoBehaviour {

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

    string lastLabel;

    /// <summary>
    /// Update the subscription to the latest Label.
    /// </summary>
    public void UpdateSubscription() {
      Unsubscribe();
      lastLabel = Label;
      Subscribe();
    }

    /// <summary>
    /// Unsubscribe from the Notifier with this listener's data.
    /// </summary>
    public void Unsubscribe() {
      Notifier.RemoveSubscriber(Callback, lastLabel, Scope);
    }

    /// <summary>
    /// Subscribe to the Notifier with this listener's data.
    /// </summary>
    void Subscribe() {
      Notifier.AddSubscriber(Callback, lastLabel, Scope);
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
  }
}
