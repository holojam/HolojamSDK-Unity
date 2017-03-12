// Spectator.cs
// Created by Holojam Inc. on 11.03.17

using UnityEngine;

[RequireComponent(typeof(Camera))]
public class Spectator : Holojam.Tools.Trackable {

  public int targetIndex = 1;

  Vector3 spectatorPosition;
  Quaternion spectatorRotation;
  bool lastTracked = false;

  public override string Label {
    get { return Holojam.Network.Canon.IndexToLabel(targetIndex); }
  }

  protected override void UpdateTracking() {
    base.UpdateTracking();

    if (Tracked != lastTracked) {
      if (Tracked) {
        // Save position and rotation
        spectatorPosition = transform.position;
        spectatorRotation = transform.rotation;
      } else {
        // Load position and rotation
        transform.position = spectatorPosition;
        transform.rotation = spectatorRotation;
      }
    }

    lastTracked = Tracked;
  }
}
