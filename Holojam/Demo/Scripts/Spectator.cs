// Spectator.cs
// Created by Holojam Inc. on 11.03.17

using UnityEngine;

[RequireComponent(typeof(Camera))]
public class Spectator : Holojam.Tools.Trackable {

  public int targetIndex = 1;
  public string spectatorLabel = "Spectator";

  public bool autoCycle = false;
  public float cycleInterval = 4; //s

  Vector3 spectatorPosition;
  Quaternion spectatorRotation;
  bool lastTracked = false;
  float lastTime;

  // Use canon label, unless target index is not valid (then use spectator label)
  public override string Label {
    get {
      return targetIndex > 0 ? Holojam.Network.Canon.IndexToLabel(targetIndex) :
        spectatorLabel;
    }
  }

  protected override void UpdateTracking() {
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

    base.UpdateTracking();
    lastTracked = Tracked;

    if (autoCycle) {
      int count = Holojam.Network.Controller.All<Holojam.Tools.Actor>().Count + 1;

      // Cycle all perspectives
      if (Time.time > lastTime + cycleInterval) {
        targetIndex = (targetIndex + 1) % count;
        lastTime = Time.time;
      }
    }
  }
}
