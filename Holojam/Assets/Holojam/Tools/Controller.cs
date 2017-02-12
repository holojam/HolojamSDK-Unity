// Controller.cs
// Created by Aaron C Gaudette on 11.11.16

using UnityEngine;

namespace Holojam.Tools {
  /// <summary>
  /// Core, adaptive class for sending and receiving a single Holojam network flake.
  /// Extend to implement custom functionality. See examples.
  /// </summary>
  [ExecuteInEditMode, RequireComponent(typeof(Network.View))]
  public abstract class Controller : MonoBehaviour {

    /// <summary>
    /// A single string combining the scope and label.
    /// </summary>
    public string Brand {
      get { return Scope + "." + Label; }
    }

    // Update function delegate
    protected delegate void ProcessDelegate();
    /// <summary>
    /// Override this delegate--will be called on Update().
    /// </summary>
    protected abstract ProcessDelegate Process { get; }

    /// <summary>
    /// Deprecated.
    /// </summary>
    public Network.View view {
      //Undefined selection
      get { return GetComponent<Network.View>(); }
    }

    // Override these to modify view behavior

    /// <summary>
    /// Override this to control the flake label (string identifier).
    /// </summary>
    public abstract string Label { get; }

    /// <summary>
    /// Override this to control the flake scope (string namespace).
    /// </summary>
    public abstract string Scope { get; }

    /// <summary>
    /// Override this to control whether or not this flake is being sent
    /// upstream by Unity (true) or updated locally from the server (false).
    /// </summary>
    public abstract bool Sending { get; }

    /// <summary>
    /// Is this an event?
    /// </summary>
    public virtual bool IgnoringTracking { get { return false; } }

    /// <summary>
    /// Override to specify vector3 allocation amount (-1 = null/optional field) for this flake.
    /// </summary>
    public virtual int TripleCount { get { return -1; } }

    /// <summary>
    /// Override to specify vector4 allocation amount (-1 = null/optional field) for this flake.
    /// </summary>
    public virtual int QuadCount { get { return -1; } }

    /// <summary>
    /// Override to specify float allocation amount (-1 = null/optional field) for this flake.
    /// </summary>
    public virtual int FloatCount { get { return -1; } }

    /// <summary>
    /// Override to specify int allocation amount (-1 = null/optional field) for this flake.
    /// </summary>
    public virtual int IntCount { get { return -1; } }

    /// <summary>
    /// Override to specify char allocation amount (-1 = null/optional field) for this flake.
    /// </summary>
    public virtual int CharCount { get { return -1; } }

    /// <summary>
    /// Override to specify whether or not this flake has a data string.
    /// </summary>
    public virtual bool HasText { get { return false; } }

    /// <summary>
    /// Force update the View (by default done on Update()).
    /// </summary>
    public void UpdateView() {
      view.label = Label;
      view.scope = Scope;
      view.sending = Sending;
      view.ignoreTracking = IgnoringTracking;
    }

    /// <summary>
    /// Call this to reset fields and re-allocate vectors.
    /// e.g. on a label change
    /// </summary>
    public void ResetView() {
      view.triples = TripleCount < 0 ? null : new Vector3[TripleCount];
      view.quads = QuadCount < 0 ? null : new Quaternion[QuadCount];
      view.floats = FloatCount < 0 ? null : new float[FloatCount];
      view.ints = IntCount < 0 ? null : new int[IntCount];
      view.chars = CharCount < 0 ? null : new byte[CharCount];
      view.text = HasText ? null : "";
    }

    /// <summary>
    /// Set a vector3 in the flake, given that it has been allocated (no bounds checks).
    /// </summary>
    /// <param name="index"></param>
    /// <param name="input"></param>
    protected void SetTriple(int index, Vector3 input) { view.triples[index] = input; }

    /// <summary>
    /// Set a vector4 in the flake, given that it has been allocated (no bounds checks).
    /// </summary>
    /// <param name="index"></param>
    /// <param name="input"></param>
    protected void SetQuad(int index, Quaternion input) { view.quads[index] = input; }

    /// <summary>
    /// Set a float in the flake, given that it has been allocated (no bounds checks).
    /// </summary>
    /// <param name="index"></param>
    /// <param name="input"></param>
    protected void SetFloat(int index, float input) { view.floats[index] = input; }

    /// <summary>
    /// Set an int in the flake, given that it has been allocated (no bounds checks).
    /// </summary>
    /// <param name="index"></param>
    /// <param name="input"></param>
    protected void SetInt(int index, int input) { view.ints[index] = input; }

    /// <summary>
    /// Set a char in the flake, given that it has been allocated (no bounds checks).
    /// </summary>
    /// <param name="index"></param>
    /// <param name="input"></param>
    protected void SetChar(int index, byte input) { view.chars[index] = input; }

    /// <summary>
    /// Set the string in the flake, given that it has been allocated (no bounds checks).
    /// </summary>
    /// <param name="input"></param>
    protected void SetText(string input) { view.text = input; }

    /// <summary>
    /// Get a vector3 from the flake, given that it has been allocated (no bounds checks). 
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    protected Vector3 GetTriple(int index) { return view.triples[index]; }

    /// <summary>
    /// Get a vector4 from the flake, given that it has been allocated (no bounds checks). 
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    protected Quaternion GetQuad(int index) { return view.quads[index]; }

    /// <summary>
    /// Get a float from the flake, given that it has been allocated (no bounds checks). 
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    protected float GetFloat(int index) { return view.floats[index]; }

    /// <summary>
    /// Get an int from the flake, given that it has been allocated (no bounds checks). 
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    protected int GetInt(int index) { return view.ints[index]; }

    /// <summary>
    /// Get a char from the flake, given that it has been allocated (no bounds checks). 
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    protected byte GetChar(int index) { return view.chars[index]; }

    /// <summary>
    /// Get the text from the flake, given that it has been allocated (no bounds checks). 
    /// </summary>
    /// <returns></returns>
    protected string GetText() { return view.text; }

    /// <summary>
    /// Awake() is overriden, by default resetting the view.
    /// </summary>
    protected virtual void Awake() {
      ResetView(); // Mandatory call
    }

    /// <summary>
    /// Update() is overriden. Instead of using this function, use the ProcessDelegate.
    /// </summary>
    protected virtual void Update() {
      UpdateView(); // Mandatory call

      // Optional check--you probably don't want to run this code in edit mode
      if (!Application.isPlaying) return;

      Process();
    }
  }
}
