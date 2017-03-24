// EventIO.cs
// Created by Holojam Inc. on 16.02.17

using UnityEngine;

namespace Holojam.Network {

  /// <summary>
  /// Abstract base class for routing data on an event out through another event. 
  /// See Metrics.cs for an example.
  /// <typeparam name="I">The input listener, which must be an EventInput.</typeparam>
  /// <typeparam name="O">The output pusher, which must be an EventPusher.</typeparam>
  /// </summary>
  public abstract class EventIO<I,O> : MonoBehaviour
    where I : EventInput<I,O> where O : EventPusher {

    /// <summary>
    /// The input listener, which will trigger OnInput().
    /// </summary>
    protected I input;

    /// <summary>
    /// The output pusher, which can be triggered via Push().
    /// </summary>
    protected O output;

    /// <summary>
    /// The function called when the input event is triggered.
    /// </summary>
    protected abstract void OnInput(string source, string scope, Flake input);

    internal void RouteIO(string source, string scope, Flake input) {
      OnInput(source, scope, input);
    }

    /// <summary>
    /// Initialize input and output components.
    /// </summary>
    protected void AddIO() {
      input = gameObject.AddComponent<I>() as I;
      // Wait to update the subscription until the input knows we're here
      input.io = this;
      input.UpdateSubscription();
      output = gameObject.AddComponent<O>() as O;
    }

    /// <summary>
    /// Clean (remove) input and output components.
    /// </summary>
    protected void RemoveIO() {
      Destroy(input);
      Destroy(output);
    }

    /// <summary>
    /// Awake() is overriden, by default initializing the input and output components.
    /// </summary>
    protected virtual void Awake() {
      AddIO();
    }
  }
}
