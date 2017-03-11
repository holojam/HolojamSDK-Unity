// SynchronizableTrackable.cs
// Created by Holojam Inc. on 19.02.17

namespace Holojam.Tools {

  /// <summary>
  /// Alternative Synchronizable with Trackable (managed position and rotation) functionality.
  /// In lieu of multiple inheritance. Extend from here if you're dealing with moving, rotating,
  /// objects.
  /// </summary>
  public abstract class SynchronizableTrackable : Trackable {

    /// <summary>
    /// Is this machine hosting the data (true) or receiving it (false)?
    /// Overriden by AutoHost.
    /// </summary>
    public abstract bool Host { get; }

    /// <summary>
    /// If true, forces Host to true on a master client and vice versa.
    /// </summary>
    public abstract bool AutoHost { get; }

    /// <summary>
    /// Sending flag for the Synchronizable, dependent on Host and AutoHost.
    /// </summary>
    public sealed override bool Sending {
      get { return AutoHost ? BuildManager.IsMasterClient() : Host; }
    }

    /// <summary>
    /// Override this in derived classes to manage your synchronized data.
    /// Synchronizes Trackable data by default.
    /// </summary>
    protected virtual void Sync() {
      if (Sending) {
        RawPosition = transform.position;
        RawRotation = transform.rotation;
      } else {
        transform.position = TrackedPosition;
        transform.rotation = TrackedRotation;
      }
    }

    /// <summary>
    /// UpdateTracking() is replaced by Sync().
    /// </summary>
    protected sealed override void UpdateTracking() { Sync(); }
  }
}
