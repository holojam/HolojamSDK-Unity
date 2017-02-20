// IEvent.cs
// Created by Holojam Inc. on 16.02.17

using System.Collections;

namespace Holojam.Network {

  /// <summary>
  /// Interface with shared properties between EventListener and EventPusher.
  /// </summary>
  public interface IEvent {
    string Label { get; }

    /// <summary>
    /// A single string combining the scope and label.
    /// </summary>
    string Brand { get; }

    #if UNITY_EDITOR
    /// <summary>
    /// Used for display (View) purposes in the editor.
    /// </summary>
    bool Fired { get; }
    #endif
  }
}
