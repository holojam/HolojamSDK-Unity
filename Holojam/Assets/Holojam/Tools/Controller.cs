// Controller.cs
// Created by Aaron C Gaudette on 11.11.16

using UnityEngine;

namespace Holojam.Tools {

  /// <summary>
  /// Core, adaptive class for sending and receiving a single Holojam network flake.
  /// Dependent on a Network.View. Extend to implement custom functionality. See examples.
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

    Network.View view;

    /// <summary>
    /// Override this to control the flake label (string identifier). See View.cs.
    /// </summary>
    public abstract string Label { get; }

    /// <summary>
    /// Override this to control the flake scope (string namespace). See View.cs.
    /// </summary>
    public abstract string Scope { get; }

    /// <summary>
    /// Override this to control whether or not this flake is being sent
    /// upstream by Unity (true) or updated locally from the server (false). See View.cs.
    /// </summary>
    public abstract bool Sending { get; }

    /// <summary>
    /// Is this an event (events ignore updates). See View.cs.
    /// </summary>
    public virtual bool Deaf { get { return false; } }

    /// <summary>
    /// Override to specify Vector3 allocation amount (-1 = null/optional field) for this flake.
    /// </summary>
    public virtual int TripleCount { get { return -1; } }

    /// <summary>
    /// Override to specify Vector4 allocation amount (-1 = null/optional field) for this flake.
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
    /// Override to specify whether or not this flake contains a string field.
    /// </summary>
    public virtual bool HasText { get { return false; } }

    /// <summary>
    /// Wrapper property from View: network origin. See View.cs.
    /// </summary>
    public string Source { get { return view.Source; } }

    /// <summary>
    /// Wrapper property from View: tracked flag. See View.cs.
    /// </summary>
    public bool Tracked { get { return view.Tracked; } }

    /// <summary>
    /// Force update the View (by default done on Update()).
    /// </summary>
    public void UpdateView() {
      view.label = Label;
      view.scope = Scope;
      view.sending = Sending;
      view.deaf = Deaf;
    }

    /// <summary>
    /// Call this to reset fields and re-allocate vectors.
    /// e.g. on a label change
    /// </summary>
    public void ResetView() {
      view = GetComponent<Network.View>(); // Unsafe

      view.triples = TripleCount < 0 ? null : new Vector3[TripleCount];
      view.quads = QuadCount < 0 ? null : new Quaternion[QuadCount];
      view.floats = FloatCount < 0 ? null : new float[FloatCount];
      view.ints = IntCount < 0 ? null : new int[IntCount];
      view.chars = CharCount < 0 ? null : new byte[CharCount];
      view.text = HasText ? null : "";
    }

    /// <summary>
    /// Set a Vector3 in the flake, given that it has been allocated (no bounds checks).
    /// </summary>
    /// <param name="index"></param>
    /// <param name="input"></param>
    protected void SetTriple(int index, Vector3 input) { view.triples[index] = input; }

    /// <summary>
    /// Set a Vector4 in the flake, given that it has been allocated (no bounds checks).
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
    /// Get a Vector3 from the flake, given that it has been allocated (no bounds checks). 
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    protected Vector3 GetTriple(int index) { return view.triples[index]; }

    /// <summary>
    /// Get a Vector4 from the flake, given that it has been allocated (no bounds checks). 
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
    /// Push the View to the Holojam network as an event.
    /// </summary>
    protected void PushEvent(){
      Network.Client.PushEvent(view);
    }

    /// <summary>
    /// Awake() is overriden, by default resetting the View.
    /// </summary>
    protected virtual void Awake() {
      ResetView();
    }

    /// <summary>
    /// Update() is overriden, by default updating the View.
    /// Instead of using this function, use the ProcessDelegate.
    /// </summary>
    protected virtual void Update() {
      UpdateView(); // Mandatory call

      // Optional check--you probably don't want to run this code in edit mode
      if (!Application.isPlaying) return;

      Process();
    }
  }
}
