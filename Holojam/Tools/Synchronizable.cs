// Synchronizable.cs
// Created by Holojam Inc. on 11.07.16

using UnityEngine;

namespace Holojam.Tools {

  /// <summary>
  /// Base class for synchronizing network data between multiple clients, where one host
  /// broadcasts data to one or more non-hosts.
  /// When extending this class, don't forget to override ResetData()!
  /// Also see SynchronizableTrackable.
  /// </summary>
  public abstract class Synchronizable : Network.Controller {

    protected sealed override ProcessDelegate Process { get { return Sync; } }

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
    /// </summary>
    protected abstract void Sync();
  }
}
