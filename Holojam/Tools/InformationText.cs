// InformationText.cs
// Created by Holojam Inc. on 31.05.17

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
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
    SortedDictionary<string, string> vrTextToShow = new SortedDictionary<string, string>();
    /// <summary>
    /// Mesh for the text shown in the VR headset.
    /// </summary>
    TextMesh vrText;

    // TODO: DOC ALL NEW THINGS
    [SerializeField] KeyCode toggleWindowTextKey = KeyCode.BackQuote;

    // TODO: DOC ALL NEW THINGS
    SortedDictionary<string, string> windowTextToShow = new SortedDictionary<string, string>();
    Text windowText;
    bool showWindowText = false;

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

      // Attach it to the main camera, if available, which should be the VR camera in VR builds
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

      // Create a Unity UI panel for the window text (this shouldn't show in VR)
      GameObject canvasObject = new GameObject("Window Information Text Canvas");
      Canvas canvas = canvasObject.AddComponent<Canvas>();
      canvas.renderMode = RenderMode.ScreenSpaceOverlay;
      canvasObject.AddComponent<CanvasScaler>();
      canvasObject.AddComponent<GraphicRaycaster>();
      GameObject windowTextObject = new GameObject("Window Information Text");
      windowTextObject.AddComponent<CanvasRenderer>();
      windowText = windowTextObject.AddComponent<Text>();
      windowText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
      windowText.fontSize = 18;
      windowTextObject.transform.SetParent(canvasObject.transform);
      windowText.rectTransform.anchorMin = Vector2.zero;
      windowText.rectTransform.anchorMax = Vector2.one;
      windowText.rectTransform.offsetMin = Vector2.one * 10;
      windowText.rectTransform.offsetMax = Vector2.one * -10;

      showWindowText = false;
    }

    void Update() {
      if (Input.GetKeyUp(toggleWindowTextKey)) {
        showWindowText = !showWindowText;
      }

      vrText.text = ConcatenateValues(vrTextToShow);

      windowText.gameObject.SetActive(showWindowText);
      if (showWindowText) {
        windowText.text = ConcatenateValues(windowTextToShow);
      }
    }

    string ConcatenateValues(SortedDictionary<string, string> textSnippets) {
      string finalText = "";
      foreach (var textSnippet in textSnippets) {
        if (finalText != "") {
          finalText += "\n";
        }
        finalText += textSnippet.Value;
      }
      return finalText;
    }

    public static void SetWindowText(string key, string text) {
      global.windowTextToShow[key] = text;
    }

    public static void ClearWindowText(string key) {
      global.windowTextToShow.Remove(key);
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