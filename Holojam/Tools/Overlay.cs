// Overlay.cs
// Created by Holojam Inc. on 15.04.17

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Holojam.Tools {

  /// <summary>
  /// Component that displays text as an overlay in VR.
  /// The overlay is displayed in front of the main camera, in both the VR headset and
  /// the main window (if applicable).
  /// </summary>
  public class Overlay : Utility.Global<Overlay> {

    /// <summary>
    /// The size of the text.
    /// </summary>
    const int FONT_SIZE = 50;

    /// <summary>
    /// The scale of the text mesh object.
    /// </summary>
    readonly Vector3 SCALE = new Vector3(0.01f, 0.01f, 0.01f);

    /// <summary>
    /// The position of the text relative to the camera.
    /// </summary>
    readonly Vector3 RELATIVE_POSITION = Vector3.forward;

    /// <summary>
    /// Sorted dictionary of strings to show in the overlay.
    /// </summary>
    SortedDictionary<string, string> snippets = new SortedDictionary<string, string>();

    /// <summary>
    /// Mesh for the (3D) text shown in the VR headset.
    /// </summary>
    TextMesh text;

    /// <summary>
    /// Sets a string to be displayed to users on the overlay (in VR).
    /// </summary>
    /// <param name="key">
    /// A unique key to identify this string. The key can be used to update
    /// this string again or to remove the text (by calling ClearOverlayKey).
    /// </param>
    /// <param name="text">The text you want to display to the user.</param>
    public static void SetOverlayKey(string key, string text) {
      if (global)
        global.snippets[key] = text;
    }

    /// <summary>
    /// Removes a string from the VR/main camera view.
    /// </summary>
    /// <param name="key">The key  corresponding to the string you want to remove.</param>
    public static void ClearOverlayKey(string key) {
      if (global)
        global.snippets.Remove(key);
    }

    void Start() {
      // Create the 3D text object for the VR text overlay
      GameObject o = new GameObject("HolojamInfoOverlay");
      text = o.AddComponent<TextMesh>();
      text.text = "";
      text.anchor = TextAnchor.MiddleCenter;
      text.alignment = TextAlignment.Center;
      text.fontSize = FONT_SIZE;
      text.transform.localScale = SCALE;

      // Attach it to the main camera, if available, which should be the VR camera in VR builds
      Camera mainCamera = Camera.main;
      if (mainCamera == null)
        o.SetActive(false);

      else if (o.transform.parent != mainCamera.transform) {
        o.SetActive(true);
        o.transform.parent = mainCamera.transform;
        o.transform.localPosition = RELATIVE_POSITION;
        o.transform.localRotation = Quaternion.identity;
      }
    }

    void Update() {
      text.text = ConcatenateSnippets(snippets.Values);
    }

    /// <summary>
    /// Utility function to concatenate an enumerable of strings.
    /// </summary>
    /// <returns>
    /// A single string with all the values concatenated, separated by newlines.
    /// </returns>
    /// <param name="snippets">
    /// The IEnumerable whose values you want to concatenate.
    /// </param>
    string ConcatenateSnippets(IEnumerable<string> snippets) {
      var it = snippets.GetEnumerator();
      it.MoveNext();
      string s = it.Current;

      while (it.MoveNext())
        s += "\n" + it.Current;

      return s;
    }
  }
}
