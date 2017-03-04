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
    /// Returns a Holojam standard actor label (string), given an input build index.
    /// </summary>
    /// <param name="index"></param>
    /// <param name="extra">Optional string to append to the end of the label. </param>
    /// <returns>The string specified.</returns>
    public static string IndexToLabel(int index, string extra = "") {
      return "M" + Mathf.Max(1, index) + (string.IsNullOrEmpty(extra) ? "" : "-" + extra);
    }
  }
}
