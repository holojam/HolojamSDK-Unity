// ViveModule.cs
// Created by Holojam Inc. on 09.03.17

using System;
using UnityEngine;
using Valve.VR;

namespace Holojam.Vive {

  [ExecuteInEditMode]
  public sealed class ViveModule : MonoBehaviour {

    /// <summary>
    /// Vive controller type, used for labeling.
    /// </summary>
    public enum Type { LEFT, RIGHT };

    internal static string TypeToString(Type type) {
      switch (type) {
        case Type.LEFT:
          return "left";
        case Type.RIGHT:
          return "right";
      }
      return "null";
    }

    /// <summary>
    /// SteamVR camera rig game object.
    /// </summary>
    public GameObject cameraRig;

    /// <summary>
    /// Automatically configure the SteamVR camera rig object in the scene.
    /// </summary>
    /// <returns>True if successful, false if there was an error.</returns>
    public bool Configure() {
      if (!cameraRig) {
        Debug.LogWarning("Holojam.Vive.ViveModule: Camera rig not assigned!");
        return false;
      }

      SteamVR_ControllerManager controllers =
        cameraRig.GetComponent<SteamVR_ControllerManager>() as SteamVR_ControllerManager;

      // Add/update controller relays
      Action<Type> ConfigureController = type => {
        GameObject controller =
          type == Type.LEFT ? controllers.left : controllers.right;

        if (controller) {
          ViveControllerRelay relay =
            controller.GetComponent<ViveControllerRelay>() as ViveControllerRelay;
          if (!relay) {
            relay =
              controller.AddComponent<ViveControllerRelay>() as ViveControllerRelay;
          }
          relay.type = type;

        } else {
          Debug.Log("Holojam.Vive.ViveModule: Controller not found: " + TypeToString(type));
        }
      };

      if (controllers) {
        ConfigureController(Type.LEFT);
        ConfigureController(Type.RIGHT);
      }

      // Add/update Vive relay
      SteamVR_Camera camera = cameraRig.GetComponentInChildren<SteamVR_Camera>() as SteamVR_Camera;

      if (camera) {
        if (!camera.GetComponent<Tools.ActorTransformRelay>()) {
          camera.gameObject.AddComponent<Tools.ActorTransformRelay>();
        }
      } else {
        Debug.LogWarning(
          "Holojam.Vive.ViveModule: SteamVR camera not found. "
          + "Did you assign the correct game object to the camera rig field?"
        );
        return false;
      }

      Debug.Log("Holojam.Vive.ViveModule: Configure successful!");
      return true;
    }
  }
}
