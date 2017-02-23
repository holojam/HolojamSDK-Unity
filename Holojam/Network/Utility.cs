// Utility.cs
// Created by Holojam Inc. on 13.11.16

using System;
using UnityEngine;

namespace Holojam.Network {

  /// <summary>
  /// Class for canonical/reserved Holojam network identifiers.
  /// </summary>
  public class Canon {

    /// <summary>
    /// Returns a string representing the Holojam machine identifier.
    /// </summary>
    /// <returns>The string specified.</returns>
    public static string Origin() {
      return System.Environment.UserName + "@" + System.Environment.MachineName;
    }

    /// <summary>
    /// Returns a string label given a build/actor index (Holojam standard).
    /// </summary>
    /// <param name="index"></param>
    /// <param name="raw"></param>
    /// <returns>The string specified.</returns>
    public static string IndexToLabel(int index, bool raw = false, string extra = "") {
      return "M" + Mathf.Max(1, index) + (raw ? "-Raw" : (string.IsNullOrEmpty(extra) ? "" : "-" + extra));
    }
  }
}
