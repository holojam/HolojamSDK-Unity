// InformationText.cs
// Created by Holojam Inc. on 31.04.17

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Holojam.Tools {

  /// <summary>
  /// Component that displays text for debugging and error handling purposes.
  /// An information panel is instantiated automatically and toggles via a
  /// customizable key, rendering on the window.
  /// Overlay text is displayed in front of the main camera, in both the VR headset and 
  /// the main window (if applicable).
  /// </summary>
  public class InformationText : Utility.Global<InformationText> {

    /// <summary>
    /// The size of the overlay text.
    /// </summary>
    const int OVERLAY_FONT_SIZE = 50;

    /// <summary>
    /// The size of the info panel text;
    /// </summary>
    const int PANEL_FONT_SIZE = 32;

    /// <summary>
    /// The opacity of the panel background.
    /// </summary>
    const float PANEL_BG_ALPHA = .7f;

    /// <summary>
    /// The scale of the overlay (text mesh) object.
    /// </summary>
    readonly Vector3 OVERLAY_SCALE = new Vector3(0.01f, 0.01f, 0.01f);

    /// <summary>
    /// The position of the overlay text relative to the camera.
    /// </summary>
    readonly Vector3 OVERLAY_RELATIVE_POSITION = Vector3.forward;

    /// <summary>
    /// Toggles the info panel (non-VR).
    /// </summary>
    public bool displayPanel = false;

    /// <summary>
    /// The key to toggle the info panel on and off.
    /// </summary>
    [SerializeField] KeyCode togglePanelKey = KeyCode.BackQuote;

    /// <summary>
    /// Sorted dictionary of strings to show in the overlay.
    /// </summary>
    SortedDictionary<string, string> overlaySnippets = new SortedDictionary<string, string>();

    /// <summary>
    /// Mesh for the (3D) text shown in the VR headset.
    /// </summary>
    TextMesh overlayText;

    /// <summary>
    /// Sorted dictionary of strings to show in the panel.
    /// </summary>
    SortedDictionary<string, string> panelSnippets = new SortedDictionary<string, string>();

    /// <summary>
    /// Parent canvas object for the panel text.
    /// </summary>
    GameObject panel;

    /// <summary>
    /// UI object for the panel text (shown only in the window).
    /// </summary>
    Text panelText;

    /// <summary>
    /// Sets a string to be displayed to users on the info panel (not in VR).
    /// </summary>
    /// <param name="key">
    /// A unique key to identify this string. The key can be used to update
    /// this string again or to remove the text (by calling ClearPanelKey).
    /// </param>
    /// <param name="text">The text you want to display to the user.</param>
    public static void SetPanelKey(string key, string text) {
      global.panelSnippets[key] = text;
    }

    /// <summary>
    /// Removes a string from the info panel.
    /// </summary>
    /// <param name="key">The key corresponding to the string you want to remove.</param>
    public static void ClearPanelKey(string key) {
      global.panelSnippets.Remove(key);
    }

    /// <summary>
    /// Sets a string to be displayed to users on the overlay (in VR).
    /// </summary>
    /// <param name="key">
    /// A unique key to identify this string. The key can be used to update
    /// this string again or to remove the text (by calling ClearOverlayKey).
    /// </param>
    /// <param name="text">The text you want to display to the user.</param>
    public static void SetOverlayKey(string key, string text) {
      global.overlaySnippets[key] = text;
    }

    /// <summary>
    /// Removes a string from the VR/main camera view.
    /// </summary>
    /// <param name="key">The key  corresponding to the string you want to remove.</param>
    public static void ClearOverlayKey(string key) {
      global.overlaySnippets.Remove(key);
    }

    void Start() {
      // Create the 3D text object for the VR text overlay
      GameObject o = new GameObject("HolojamInfoOverlay");
      overlayText = o.AddComponent<TextMesh>();
      overlayText.text = "";
      overlayText.anchor = TextAnchor.MiddleCenter;
      overlayText.alignment = TextAlignment.Center;
      overlayText.fontSize = OVERLAY_FONT_SIZE;
      overlayText.transform.localScale = OVERLAY_SCALE;

      // Attach it to the main camera, if available, which should be the VR camera in VR builds
      Camera mainCamera = Camera.main;
      if (mainCamera == null)
        o.SetActive(false);

      else if (o.transform.parent != mainCamera.transform) {
        o.SetActive(true);
        o.transform.parent = mainCamera.transform;
        o.transform.localPosition = OVERLAY_RELATIVE_POSITION;
        o.transform.localRotation = Quaternion.identity;
      }

      // Create a Unity UI canvas for the panel text (will not display in VR)
      panel = new GameObject("HolojamInfoPanel_Canvas");
      Canvas canvas = panel.AddComponent<Canvas>();
      canvas.renderMode = RenderMode.ScreenSpaceOverlay;
      panel.AddComponent<CanvasScaler>();
      panel.AddComponent<GraphicRaycaster>();

      // UI background

      GameObject bg = new GameObject("Background");
      bg.AddComponent<CanvasRenderer>();
      bg.transform.SetParent(panel.transform);

      LayoutGroup group = bg.AddComponent<HorizontalLayoutGroup>();
      group.padding = new RectOffset(8, 8, 8, 8);

      ContentSizeFitter fitter = bg.AddComponent<ContentSizeFitter>();
      fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
      fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

      Image image = bg.AddComponent<Image>();
      image.color = new Color(0, 0, 0, PANEL_BG_ALPHA);
      image.rectTransform.pivot = Vector2.zero;

      image.rectTransform.anchoredPosition = Vector2.zero;
      image.rectTransform.anchorMin = Vector2.zero;
      image.rectTransform.anchorMax = Vector2.zero;

      // UI text

      o = new GameObject("Text");
      o.AddComponent<CanvasRenderer>();
      o.transform.SetParent(bg.transform);

      panelText = o.AddComponent<Text>();
      panelText.font = Resources.Load<Font>("Calcon");
      panelText.fontSize = PANEL_FONT_SIZE;

      displayPanel = false;
    }

    void Update() {
      if (Input.GetKeyUp(togglePanelKey))
        displayPanel = !displayPanel;

      overlayText.text = ConcatenateSnippets(overlaySnippets.Values);

      panel.SetActive(displayPanel);
      if (displayPanel)
        panelText.text = ConcatenateSnippets(panelSnippets.Values);
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
