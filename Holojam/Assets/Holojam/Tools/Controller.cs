//Controller.cs
//Created by Aaron C Gaudette on 11.11.16

using UnityEngine;

namespace Holojam.Tools {
  /// <summary>
  /// 
  /// </summary>
  [ExecuteInEditMode, RequireComponent(typeof(Network.View))]
  public abstract class Controller : MonoBehaviour {

    /// <summary>
    /// 
    /// </summary>
    public string brand {
      get {
        return scopeField + "." + labelField;
      }
    }

    //Update function delegate
    protected delegate void ProcessDelegate();
    protected abstract ProcessDelegate Process { get; }

    /// <summary>
    /// 
    /// </summary>
    public Network.View view {
      //Undefined selection
      get { return GetComponent<Network.View>(); }
    }

    //Override these to modify view behavior

    /// <summary>
    /// 
    /// </summary>
    public abstract string labelField { get; }

    /// <summary>
    /// 
    /// </summary>
    public abstract string scopeField { get; }

    /// <summary>
    /// 
    /// </summary>
    public abstract bool isSending { get; }

    /// <summary>
    /// Is this an event?
    /// </summary>
    public virtual bool isIgnoringTracking { get { return false; } }

    //Override to select allocation amount (-1 = null/optional field)

    /// <summary>
    /// 
    /// </summary>
    public virtual int tripleCount { get { return -1; } }

    /// <summary>
    /// 
    /// </summary>
    public virtual int quadCount { get { return -1; } }

    /// <summary>
    /// 
    /// </summary>
    public virtual int floatCount { get { return -1; } }

    /// <summary>
    /// 
    /// </summary>
    public virtual int intCount { get { return -1; } }

    /// <summary>
    /// 
    /// </summary>
    public virtual int charCount { get { return -1; } }

    /// <summary>
    /// 
    /// </summary>
    public virtual bool hasText { get { return false; } }

    /// <summary>
    /// 
    /// </summary>
    public void UpdateView() {
      view.label = labelField;
      view.scope = scopeField;
      view.sending = isSending;
      view.ignoreTracking = isIgnoringTracking;
    }

    //Call this to reset fields and re-allocate vectors
    //e.g. on a label change
    /// <summary>
    /// 
    /// </summary>
    public void ResetView() {
      view.triples = tripleCount < 0 ? null : new Vector3[tripleCount];
      view.quads = quadCount < 0 ? null : new Quaternion[quadCount];
      view.floats = floatCount < 0 ? null : new float[floatCount];
      view.ints = intCount < 0 ? null : new int[intCount];
      view.chars = charCount < 0 ? null : new byte[charCount];
      view.text = hasText ? null : "";
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="index"></param>
    /// <param name="input"></param>
    public void SetTriple(int index, Vector3 input) { view.triples[index] = input; }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="index"></param>
    /// <param name="input"></param>
    public void SetQuad(int index, Quaternion input) { view.quads[index] = input; }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="index"></param>
    /// <param name="input"></param>
    public void SetFloat(int index, float input) { view.floats[index] = input; }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="index"></param>
    /// <param name="input"></param>
    public void SetInt(int index, int input) { view.ints[index] = input; }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="index"></param>
    /// <param name="input"></param>
    public void SetChar(int index, byte input) { view.chars[index] = input; }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="input"></param>
    public void SetText(string input) { view.text = input; }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public Vector3 GetTriple(int index) { return view.triples[index]; }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public Quaternion GetQuad(int index) { return view.quads[index]; }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public float GetFloat(int index) { return view.floats[index]; }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public int GetInt(int index) { return view.ints[index]; }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public byte GetChar(int index) { return view.chars[index]; }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public string GetText() { return view.text; }

    /// <summary>
    /// 
    /// </summary>
    protected virtual void Awake() {
      ResetView(); //Mandatory call
    }

    /// <summary>
    /// 
    /// </summary>
    protected virtual void Update() {
      UpdateView(); //Mandatory call

      //Optional check--you probably don't want to run this code in edit mode
      if (!Application.isPlaying) return;

      Process();
    }
  }
}
