// TrackableComponent.cs
// Created by Holojam Inc. on 12.02.17
// Example Trackable

using UnityEngine;

public class TrackableComponent : Holojam.Tools.Trackable {

  [SerializeField] string label = "Trackable";
  [SerializeField] string scope = ""; 

  public override string Label { get { return label; } }
  public override string Scope { get { return scope; } }
}
