using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

namespace Holojam.Vive {
  public class ViveControllerReceiver : Tools.Trackable {

    /// <summary>
    /// Label for the controller.
    /// </summary>
    public string label;

    private int[] previousFrame = new int[6];
    private int[] currentFrame = new int[6];

    private Vector2 previousTouchpadAxis;
    private Vector2 previousTriggerAxis;

    /// <summary>
    /// Canonical button to int dictionary. The ints represent the indices of each button in the
    /// Flake's int buffer.
    /// </summary>
    private Dictionary<EVRButtonId, int> buttonToInts = new Dictionary<EVRButtonId, int>() {
      { EVRButtonId.k_EButton_ApplicationMenu, 0 },
      { EVRButtonId.k_EButton_Grip, 1 },
      { EVRButtonId.k_EButton_SteamVR_Touchpad, 2 },
      { EVRButtonId.k_EButton_SteamVR_Trigger, 3 }
    };

    /// <summary>
    /// Automatically selects the canon Vive label depending on the build index, with an additional controller label.
    /// </summary>
    public override string Label {
      get { return label; }
    }

    /// <summary>
    /// Data descriptor is initialized with one Vector3 and one Quaternion.
    /// </summary>
    public override void ResetData() {
      data = new Network.Flake(1, 1, 4, 6);
    }

    /// <summary>
    /// Property that returns a Vector2 of the touchpad axis values of the controller.
    /// </summary>
    public Vector2 TouchpadAxis {
      get { return new Vector2(data.floats[0], data.floats[1]); }
    }

    /// <summary>
    /// Property that returns a Vector2 of the trigger axis values of the controller.
    /// The x field is the only field used.
    /// </summary>
    public Vector2 TriggerAxis {
      get { return new Vector2(data.floats[2], data.floats[3]); }
    }

    /// <summary>
    /// Get whether or not the a controller's button is pressed down this frame.
    /// </summary>
    /// <param name="id">The id of the button.</param>
    /// <returns>Whether or not the button has been pressed down this frame.</returns>
    public bool GetPressDown(EVRButtonId id) {
      int index = buttonToInts[id];
      return previousFrame[index] == 0 && currentFrame[index] == 1;
    }

    /// <summary>
    /// Get whether or not a controller's button is being held this frame.
    /// </summary>
    /// <param name="id">The id of the button.</param>
    /// <returns>Whether or not the button is being held this frame.</returns>
    public bool GetPress(EVRButtonId id) {
      int index = buttonToInts[id];
      return previousFrame[index] == 1 && currentFrame[index] == 1;
    }

    /// <summary>
    /// Get whether or not a controller's button was released this frame.
    /// </summary>
    /// <param name="id">THe id of the button.</param>
    /// <returns>Whether or not the button was released this frame.</returns>
    public bool GetPressUp(EVRButtonId id) {
      int index = buttonToInts[id];
      return previousFrame[index] == 1 && currentFrame[index] == 0;
    }

    /// <summary>
    /// Get whether or not a controller's button was touched this frame.
    /// Only works for Touchpad and Trigger. All other buttons will always return false.
    /// </summary>
    /// <param name="id">The id of the button.</param>
    /// <returns>Whether or not the button was touched down this frame.</returns>
    public bool GetTouchDown(EVRButtonId id) {
      int index = buttonToInts[id] + 2;
      if (index < 4) return false;

      return previousFrame[index] == 0 && currentFrame[index] == 1;
    }

    /// <summary>
    /// Get whether or not a controller's button is being touch held this frame.
    /// Only works for Touchpad and Trigger. All other buttons will always return false.
    /// </summary>
    /// <param name="id">The id of the button.</param>
    /// <returns>Whether or not the button was touch held this frame.</returns>
    public bool GetTouch(EVRButtonId id) {
      int index = buttonToInts[id] + 2;
      if (index < 4) return false;

      return previousFrame[index] == 1 && currentFrame[index] == 1;
    }

    /// <summary>
    /// Get whether or not a controller's button was touch released this frame.
    /// Only works for Touchpad and Trigger. All other buttons will always return false.
    /// </summary>
    /// <param name="id">The id of the button.</param>
    /// <returns>Whether or not the button was touch released this frame.</returns>
    public bool GetTouchUp(EVRButtonId id) {
      int index = buttonToInts[id] + 2;
      if (index < 4) return false;

      return previousFrame[index] == 1 && currentFrame[index] == 0;
    }

    /// <summary>
    /// Get whether or not a controller's button was clicked this frame.
    /// Special context for Trigger. Otherwise, returns the results of GetPressDown.
    /// </summary>
    /// <param name="id">The id of the button.</param>
    /// <returns>Whether or not the button was clicked this frame.</returns>
    public bool GetClick(EVRButtonId id) {
      if (id == EVRButtonId.k_EButton_SteamVR_Trigger) {
        return previousTriggerAxis.x < 1 && TriggerAxis.x == 1;
      } 
      return GetPressDown(id);
    }

    protected override void UpdateTracking() {
      base.UpdateTracking();
      previousFrame = currentFrame;
      currentFrame = data.ints;

      previousTouchpadAxis = TouchpadAxis;
      previousTriggerAxis = TriggerAxis;

    }
  }
}
