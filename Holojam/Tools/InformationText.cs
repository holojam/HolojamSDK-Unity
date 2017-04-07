// InformationText.cs
// Created by Holojam Inc. on 31.05.17

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Holojam.Network;

namespace Holojam.Tools {
  /// <summary>
  /// Utility component that shows text for debugging and error handling purposes.
  /// </summary>
  public class InformationText : Utility.Global<InformationText> {

    /// <summary>
    /// Dictionary that contains the strings to show to the user in their VR headset.
    /// Text is shown hovering in front of the main camera, in both the VR headset and 
    /// the main window (if applicable).
    /// </summary>
    Dictionary<string, string> vrTextToShow = new Dictionary<string, string>();
    /// <summary>
    /// Mesh for the text shown in the VR headset.
    /// </summary>
    TextMesh vrText;

    /// <summary>
    /// The size of the VR text.
    /// </summary>
    const int VR_TEXT_FONT_SIZE = 50;
    /// <summary>
    /// The scale of the VR text object.
    /// </summary>
    Vector3 VR_TEXT_DEFAULT_SCALE = new Vector3(0.01f, 0.01f, 0.01f);
    /// <summary>
    /// The position of the VR text relative to the camera.
    /// </summary>
    Vector3 VR_TEXT_RELATIVE_POSITION = Vector3.forward;

    void Start() {
      // Create the 3D text object for the VR text
      GameObject vrTextObject = new GameObject("VR Information Text");
      vrText = vrTextObject.AddComponent<TextMesh>();
      vrText.text = "";
      vrText.anchor = TextAnchor.MiddleCenter;
      vrText.alignment = TextAlignment.Center;
      vrText.fontSize = VR_TEXT_FONT_SIZE;
      vrText.transform.localScale = VR_TEXT_DEFAULT_SCALE;

      Camera mainCamera = Camera.main;
      if (mainCamera == null) {
        vrText.gameObject.SetActive (false);
      }
      else if (vrText.transform.parent != mainCamera.transform) {
        vrText.gameObject.SetActive (true);
        vrText.transform.parent = mainCamera.transform;
        vrText.transform.localPosition = VR_TEXT_RELATIVE_POSITION;
        vrText.transform.localRotation = Quaternion.identity;
      }
    }

    void Update() {
      vrText.text = "";
      foreach (var textSnippet in vrTextToShow) {
        if (vrText.text != "") {
          vrText.text += "\n";
        }
        vrText.text += textSnippet.Value;
      }
    }

    /// <summary>
    /// Sets a string to be displayed to users in front of the main camera, both in the VR headset
    /// (in client builds) and in the window (in other builds).
    /// </summary>
    /// <param name="key">A unique key to identify this string. This key can be used to then update
    /// this string (by calling this method again with the same key) or to remove the text
    /// (by calling ClearVRText with the same key).</param>
    /// <param name="text">The text you want to display to the user.</param>
    public static void SetVRText(string key, string text) {
      global.vrTextToShow[key] = text;
    }

    /// <summary>
    /// Removes a string from the VR/main camera view.
    /// </summary>
    /// <param name="key">The key for the string you want to remove.</param>
    public static void ClearVRText(string key) {
      global.vrTextToShow.Remove(key);
    }
  }
}