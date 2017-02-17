// Controller.cs
// Created by Aaron C Gaudette on 11.11.16

using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

namespace Holojam.Network {

  /// <summary>
  /// Flexible core class for sending, managing, and receiving Holojam data.
  /// Contains a Network.Flake. Extend to implement custom functionality.
  /// See examples and Flake.cs.
  /// Executes in edit mode for editor instance management.
  /// </summary>
  [ExecuteInEditMode]
  public abstract class Controller : FlakeComponent {

    /// <summary>
    /// Static read-only list of all the specified Controller subtypes in the scene.
    /// </summary>
    /// <typeparam name="T">The Controller subtype to filter.</typeparam>
    /// <returns>A ReadOnlyCollection containing Ts.</returns>
    public static ReadOnlyCollection<T> All<T>() where T : Controller {
      List<T> filtered = new List<T>();
      // This could be slow. A better (but less flexible) solution is to override OnEnable/Disable
      foreach (Controller controller in instances)
        if (controller is T) filtered.Add(controller as T);
      return filtered.AsReadOnly();
    }

    /// <summary>
    /// Instance list made internal (unsafe) for speed within Client.cs.
    /// </summary>
    internal static List<Controller> instances = new List<Controller>();

    /// <summary>
    /// Instance management. Must call base.OnEnable() if overriding.
    /// </summary>
    void OnEnable() { instances.Add(this); }

    /// <summary>
    /// Instance management. Must call base.OnDisable() if overriding.
    /// </summary>
    void OnDisable() { instances.Remove(this); }

    /// <summary>
    /// Read-only string containing Holojam network origin (user@host).
    /// </summary>
    public string Source {
      get { return source; }
      internal set { source = value; }
    }
    string source;

    /// <summary>
    /// Read-only bool indicating if the Controller received fresh data this frame.
    /// </summary>
    public bool Tracked {
      get {
        return Application.isPlaying && instances.Contains(this)
       && (tracked || Sending);
      }
      internal set { tracked = value; }
    }
    bool tracked = false;

    /// <summary>
    /// Override this to control the data scope (string namespace).
    /// When receiving, this string is used as a filter for incoming data.
    /// An empty scope is parsed as a "whitelist all".
    /// </summary>
    public virtual string Scope { get { return ""; } }

    /// <summary>
    /// Override this to control the data label (string identifier).
    /// </summary>
    public abstract string Label { get; }

    /// <summary>
    /// Override this to control whether or not this Controller's data is being sent
    /// upstream by Unity (true) or updated locally from the server (false).
    /// </summary>
    public abstract bool Sending { get; }

    /// <summary>
    /// A single string combining the scope and label.
    /// </summary>
    public string Brand {
      get { return Scope + "." + Label; }
    }

    /// <summary>
    // Custom update function delegate.
    protected delegate void ProcessDelegate();

    /// <summary>
    /// Override this delegate--will be called on Update().
    /// </summary>
    protected abstract ProcessDelegate Process { get; }

    /// <summary>
    /// Update() is overriden. Instead of overriding this function, use the ProcessDelegate.
    /// </summary>
    protected virtual void Update() {
      // Optional check--you probably don't want to run this code in edit mode
      if (!Application.isPlaying) return;

      Process();
    }
  }
}
