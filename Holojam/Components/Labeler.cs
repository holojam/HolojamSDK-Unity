using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Holojam.Components {

  /// <summary>
  /// Static class granting access to a singleton labeler instance (NOTE: Feel
  /// free to disagree with this design choice), which generates labels upon
  /// request.
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

    public static LabelerInstance Instance {
      get {
        return (instance == null) ?
          instance = new LabelerInstance() : instance;
      }
    }
  }
}
