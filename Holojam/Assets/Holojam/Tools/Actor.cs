// Actor.cs
// Created by Holojam Inc. on 23.06.16

using UnityEngine;
using System.Collections.Generic;

namespace Holojam.Tools {
  /// <summary>
  /// Umbrella class for accessing player (headset user) data in a generic manner,
  /// decoupled from the build process and VR camera setup. This barebones base-class
  /// implementation is sufficient for tracking a head--extend for more complex use-cases.
  /// </summary>
  public class Actor : Trackable {

    /// <summary>
    /// Static list of all Actors in the scene.
    /// </summary>
    public static List<Actor> instances = new List<Actor>();

    /// <summary>
    /// Static table of all the actors from the current project.
    /// </summary>
    public static Dictionary<string, Actor> localInstances = new Dictionary<string, Actor>();

    /// <summary>
    /// The unique build index of the Actor, used in BuildManager.
    /// </summary>
    public int index;

    /// <summary>
    /// Is this actor the current build target (see BuildManager)?
    /// </summary>
    public bool IsBuild { get { return BuildManager.BUILD_ACTOR == this; } }

    /// <summary>
    /// Is this actor from the current project?
    /// </summary>
    public bool IsLocal { get { return localInstances.ContainsKey(Brand); } }

    [SerializeField] protected string scope = ""; 

    protected const Utility.Palette.Colors DEFAULT_COLOR = Utility.Palette.Colors.SEA_FOAM;

    /// <summary>
    /// The color used to render the Actor gizmo.
    /// </summary>
    public Color debugColor = Utility.Palette.Select(DEFAULT_COLOR);

    const float dropInterval = 1;
    bool lastTracked = false; float lastTime;

    /// <summary>
    /// An Actor's label is determined from its build index using the canon.
    /// </summary>
    public override string Label { get { return Network.Canon.IndexToLabel(index); } }

    void OnEnable() { instances.Add(this); }
    void OnDisable() { instances.Remove(this); }

    /// <summary>
    /// Update Actor registration information.
    /// </summary>
    void UpdateData() {
      // Update local instances dictionary
      bool local = view.scope == Network.Client.SEND_SCOPE || string.IsNullOrEmpty(view.scope);
      if (local && !localInstances.ContainsKey(Brand))
        localInstances[Brand] = this;
      else if (!local && localInstances.ContainsKey(Brand))
        localInstances.Remove(Brand);

      // Call fade events
      if (Time.time > lastTime + dropInterval) {
        if (view.tracked != lastTracked) {
          if (view.tracked) FadeIn();
          else FadeOut();
        }
        lastTracked = view.tracked;
        lastTime = Time.time;
      }
    }

    public override string Scope { get { return scope; } }

    /// <summary>
    /// See <code>Controller.cs</code> for details.
    /// </summary>
    protected override void Update() {
      base.Update();
      UpdateData(); // Mandatory call
    }

    /// <summary>
    /// See <code>Trackable.cs</code> for details.
    /// Override this in a derived class for a custom unique implementation.
    /// </summary>
    protected override void UpdateTracking() {
      base.UpdateTracking();
    }

    /// <summary>
    /// Event called on join. Override for unique implementation.
    /// </summary>
    protected virtual void FadeIn() { }

    /// <summary>
    /// Event called on tracking loss. Override for unique implementation.
    /// </summary>
    protected virtual void FadeOut() { }

    /// <summary>
    /// Provides reliable position information about the Actor instance.
    /// If overridden, this function should reference assigned data (e.g. transform.position), not raw data
    /// so that the changes made in Update() are reflected.
    /// </summary>
    public virtual Vector3 Center {
      get { return transform.position; }
    }

    /// <summary>
    /// Provides reliable orientation information about the Actor instance.
    /// If overridden, this function should reference assigned data (e.g. transform.rotation), not raw data
    /// so that the changes made in Update() are reflected.
    /// </summary>
    public virtual Quaternion Orientation {
      get { return transform.rotation; }
    }


    /// <summary>
    /// This accessor dictates where each user is looking in their headset. Override for unique
    /// edge cases--when you are manually augmenting the actor rotation or when you want
    /// the user's look direction to differ from what the actor is broadcasting (not recommended).
    /// Be careful not to map rotation to anything other than the raw data
    /// (the user's actual head movement) unless you absolutely know what you're doing.
    /// </summary>
    public virtual Quaternion RawOrientation {
      get { return RawRotation; }
    }

    /// <summary>
    /// The look vector of the Actor.
    /// </summary>
    public Vector3 Look { get { return Orientation * Vector3.forward; } }

    /// <summary>
    /// The up vector of the Actor.
    /// </summary>
    public Vector3 Up { get { return Orientation * Vector3.up; } }

    /// <summary>
    /// The left vector of the Actor.
    /// </summary>
    public Vector3 Left { get { return Orientation * Vector3.left; } }

    // Useful (goggles) visualization
    void OnDrawGizmos() {
      DrawGizmoGhost();
      Gizmos.color = debugColor;
      Vector3 offset = Center + Look * 0.0f;
      Utility.Drawer.Circle(offset + Left * 0.035f, Look, Up, 0.03f);
      Utility.Drawer.Circle(offset - Left * 0.035f, Look, Up, 0.03f);
      // Reference forward vector
      Gizmos.DrawRay(offset, Look);
    }
    void OnDrawGizmosSelected() { } // Override Trackable implementation
  }
}
