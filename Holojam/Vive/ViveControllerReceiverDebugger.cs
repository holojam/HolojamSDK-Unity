using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

namespace Holojam.Vive {
  [RequireComponent(typeof(ViveControllerReceiver))]
  public class ViveControllerReceiverDebugger : MonoBehaviour {

    private readonly string header = "Holojam.Vive.VCRD: ";

    private ViveControllerReceiver receiver;

    //Steam Controller button and axis ids
    private EVRButtonId[] pressIds = new EVRButtonId[] {
      EVRButtonId.k_EButton_ApplicationMenu,
      EVRButtonId.k_EButton_Grip,
      EVRButtonId.k_EButton_SteamVR_Touchpad,
      EVRButtonId.k_EButton_SteamVR_Trigger
    };

    private EVRButtonId[] touchIds = new EVRButtonId[] {
      EVRButtonId.k_EButton_SteamVR_Touchpad,
      EVRButtonId.k_EButton_SteamVR_Trigger
    };

    private void Awake() {
      receiver = GetComponent<ViveControllerReceiver>();
    }

    private void Update() {
      foreach (EVRButtonId id in pressIds) {
        if (receiver.GetPressDown(id)) {
          Debug.Log(header + receiver.Label + " " + id.ToString() + " GetPressDown.");
        } else if (receiver.GetPress(id)) {
          Debug.Log(header + receiver.Label + " " + id.ToString() + " GetPress.");
        } else if (receiver.GetPressUp(id)) {
          Debug.Log(header + receiver.Label + " " + id.ToString() + " GetPressUp.");
        }
      }

      foreach (EVRButtonId id in touchIds) {
        if (receiver.GetTouchDown(id)) {
          Debug.Log(header + receiver.Label + " " + id.ToString() + " GetTouchDown.");
        } else if (receiver.GetTouch(id)) {
          Debug.Log(header + receiver.Label + " " + id.ToString() + " GetTouch.");
        } else if (receiver.GetTouchUp(id)) {
          Debug.Log(header + receiver.Label + " " + id.ToString() + " GetTouchUp.");
        }
      }
    }
  }
}

