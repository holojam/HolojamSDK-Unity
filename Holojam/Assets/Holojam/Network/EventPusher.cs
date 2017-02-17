// EventPusher.cs
// Created by Holojam Inc. 16.02.17

namespace Holojam.Network {

  /// <summary>
  /// Abstract base class (boilerplate) for pushing Holojam events in Unity.
  /// Contains a Network.Flake. Extend to implement custom functionality. See examples and Flake.cs.
  /// </summary>
  public abstract class EventPusher : FlakeComponent {

    /// <summary>
    /// Label (string) to push the data under.
    /// </summary>
    public abstract string Label { get; }

    public void Push() {
      Client.PushEvent(Label, data);
    }
  }
}
