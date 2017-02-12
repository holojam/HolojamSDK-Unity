// Synchronizable.cs
// Created by Holojam Inc. on 11.07.16

using UnityEngine;

namespace Holojam.Tools {

  /// <summary>
  /// Base class for synchronizing network data between multiple clients.
  /// </summary>
  public abstract class Synchronizable : Controller {

    [SerializeField] private bool sending = true;

    /// <summary>
    /// If true, forces sending to true on a master client and vice versa.
    /// </summary>
    public bool useMasterClient = false;

    protected override ProcessDelegate Process { get { return Sync; } }

    /// <summary>
    /// Sending flag for the Synchronizable. Dependent on editor selection
    /// as well as the useMasterClient flag.
    /// </summary>
    public override bool Sending {
      get {
        return sending && (BuildManager.IsMasterClient() || !useMasterClient);
      }
    }

    //Override this in derived classes
    /// <summary>
    /// Override this in derived classes to synchronize data (dependent on the sending flag).
    /// See <code>Synchronized.cs</code> for an example.
    /// </summary>
    protected abstract void Sync();
  }
}
