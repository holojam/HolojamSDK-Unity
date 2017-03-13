// Spectator.cs
// Created by Holojam Inc. on 11.03.17

using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class Spectator : Holojam.Tools.Trackable {

  public int targetIndex = 1;
  public string spectatorLabel = "Spectator";

  public bool autoCycle = false;
  public float cycleInterval = 4; //s

  Vector3 spectatorPosition;
  Quaternion spectatorRotation;
  bool lastTracked = false, loaded = true;
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
        if (loaded) {
          // Save position and rotation
          spectatorPosition = transform.position;
          spectatorRotation = transform.rotation;
          loaded = false;
        }
      } else {
        StartCoroutine(LoadSpectator());
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

  // Only load spectator values after a timeout
  IEnumerator LoadSpectator() {
    yield return new WaitForSeconds(2);
    if (!Tracked) {
      // Load position and rotation
      transform.position = spectatorPosition;
      transform.rotation = spectatorRotation;
      loaded = true;
    }
  }
}
