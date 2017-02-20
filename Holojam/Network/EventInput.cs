// EventInput.cs
// Created by Holojam on 16.02.17

using UnityEngine;

namespace Holojam.Network {

  /// <summary>
  /// Abstract EventListener that automatically links with and triggers a master router (EventIO).
  /// </summary>
  public abstract class EventInput<I,O> : EventListener
    where I : EventInput<I,O> where O : EventPusher { 

    /// <summary>
    /// The router (master, EventIO component).
    /// </summary>
    internal EventIO<I,O> io;

    /// <summary>
    /// On event trigger, call a function on the master.
    /// </summary>
    protected override Notifier.Callback Callback { get { return io.RouteIO; } }

    /// <summary>
    /// Don't update the subscription automatically if there's no router.
    /// </summary>
    protected override void OnEnable() {
      if (io != null) base.OnEnable();
    }
  }
}
