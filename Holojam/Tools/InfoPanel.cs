// InfoPanel.cs
// Created by Holojam Inc. on 31.03.17

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Holojam.Tools {

  /// <summary>
  /// Component that displays an informational UI panel for debugging and error handling purposes.
  /// The panel is instantied automatically and toggled via a customizable key.
  /// </summary>
  public class InfoPanel : Utility.Global<InfoPanel> {

    /// The size of the info panel text;
    /// </summary>
    const int FONT_SIZE = 32;

    /// <summary>
    /// The opacity of the panel background.
    /// </summary>
    const float BG_ALPHA = .7f;

    /// <summary>
    /// Toggles the info panel (non-VR).
    /// </summary>
    public bool display = false;

    /// <summary>
    /// The key to toggle the info panel on and off.
    /// </summary>
    [SerializeField] KeyCode toggleKey = KeyCode.BackQuote;

    /// <summary>
    /// Sorted dictionary of strings to show in the panel.
    /// </summary>
    SortedDictionary<string, string> snippets = new SortedDictionary<string, string>();

    /// <summary>
    /// Parent canvas object (instantiated).
    /// </summary>
    GameObject panel;

    /// <summary>
    /// UI object for the panel text.
    /// </summary>
    Text text;

    /// <summary>
    /// Sets a string to be displayed to users on the info panel (not in VR).
    /// </summary>
    /// <param name="key">
    /// A unique key to identify this string. The key can be used to update
    /// this string again or to remove the text (by calling ClearPanelKey).
    /// </param>
    /// <param name="text">The text you want to display to the user.</param>
    public static void SetString(string key, string text) {
      if (global)
        global.snippets[key] = text;
    }

    /// <summary>
    /// Removes a string from the info panel.
    /// </summary>
    /// <param name="key">The key corresponding to the string you want to remove.</param>
    public static void ClearString(string key) {
      if (global)
        global.snippets.Remove(key);
    }

    void Start() {
      // Create a Unity UI canvas for the panel text
      panel = new GameObject("HolojamInfoPanel");
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
      image.color = new Color(0, 0, 0, BG_ALPHA);
      image.rectTransform.pivot = Vector2.zero;

      image.rectTransform.anchoredPosition = Vector2.zero;
      image.rectTransform.anchorMin = Vector2.zero;
      image.rectTransform.anchorMax = Vector2.zero;

      // UI text

      GameObject o = new GameObject("Text");
      o.AddComponent<CanvasRenderer>();
      o.transform.SetParent(bg.transform);

      text = o.AddComponent<Text>();
      text.font = Resources.Load<Font>("Calcon");
      text.fontSize = FONT_SIZE;
    }

    void Update() {
      if (Input.GetKeyUp(toggleKey))
        display = !display;

      panel.SetActive(display);
      if (display)
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
