using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Holojam.Components {

  /// <summary>
  /// Static class with functionality to generate network labels upon request.
  /// </summary>
  public static class Labeler {

    public class LabelerInstance {

      private readonly IDictionary<string, int> collisionMap;

      internal LabelerInstance() {
        collisionMap = new Dictionary<string, int>();
      }

      /// <summary>
      /// Given a GameObject, generate and return a unique label
      /// </summary>
      public string GenerateLabel(GameObject g) {
        int count = 0;

        // If the key exists, increase the count before updating the dictionary
        if (collisionMap.TryGetValue(g.name, out count)) {
          count++;
        }

        collisionMap[g.name] = count;

        // Concatenate the name with count
        return g.name + ':' + count;
      }
    }

    private static LabelerInstance instance;

    /// <summary>
    /// Wrapper for internal instance method, GenerateLabel.
    /// Returns a new label upon request.
    /// </summary>
    public static string GenerateLabel(GameObject g) {
      if (instance == null) {
        instance = new LabelerInstance();
      }

      return instance.GenerateLabel(g);
    }
  }
}
